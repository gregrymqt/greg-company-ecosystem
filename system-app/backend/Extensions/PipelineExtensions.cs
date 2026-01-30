using System.Reflection;
using Hangfire;
using MeuCrudCsharp.Data;
using MeuCrudCsharp.Documents.Attributes; // <--- CERTIFIQUE-SE QUE ESTE USING APONTA PARA ONDE CRIOU O ATRIBUTO
using MeuCrudCsharp.Documents.Interfaces; // Certifique-se que IMongoDocument está aqui
using MeuCrudCsharp.Features.Hubs;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MeuCrudCsharp.Extensions;

public static class PipelineExtensions
{
    public static async Task<WebApplication> UseAppPipeline(this WebApplication app)
    {
        // --- 1. Configuração de Erros e Segurança Básica ---
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();

        // --- 2. Inicialização do Banco de Dados ---
        await ApplyMigrations(app);
        await SeedInitialRoles(app);

        // NOVO: Chama a configuração dos índices do Mongo
        await ConfigureMongoDbIndexes(app);

        // --- 3. Roteamento e CORS ---
        app.UseRouting();
        app.UseCors(WebServicesExtensions.CorsPolicyName);

        // --- 4. Autenticação e Autorização ---
        app.UseAuthFeatures();

        // --- 5. Middlewares Específicos ---
        app.UseHangfireDashboard();

        // --- 6. Endpoints ---
        app.MapHub<VideoProcessingHub>("/videoProcessingHub");
        app.MapHub<RefundProcessingHub>("/RefundProcessingHub");
        app.MapHub<PaymentProcessingHub>("/PaymentProcessingHub");

        app.MapRazorPages();
        app.MapControllers();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }

    private static async Task SeedInitialRoles(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Roles>>();

        string[] roles = [AppRoles.Admin, AppRoles.User, AppRoles.Manager];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Roles(roleName));
            }
        }
    }

    private static async Task ApplyMigrations(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await dbContext.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Erro ao aplicar migrations: {ex.Message}");
        }
    }

    private static async Task ConfigureMongoDbIndexes(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var database = scope.ServiceProvider.GetService<IMongoDatabase>();

        if (database == null)
            return;

        // --- CORREÇÃO: Escaneie apenas o Assembly que contém seus documentos ---
        // Substitua 'IMongoDocument' por uma classe que esteja no mesmo projeto dos seus modelos
        var documentTypes = typeof(IMongoDocument)
            .Assembly.GetTypes()
            .Where(p =>
                typeof(IMongoDocument).IsAssignableFrom(p)
                && p is { IsInterface: false, IsAbstract: false }
            );

        foreach (var docType in documentTypes)
        {
            var collectionNameProperty = docType.GetProperty(
                "CollectionName",
                BindingFlags.Public | BindingFlags.Static
            );
            var collectionName = collectionNameProperty?.GetValue(null) as string;

            if (string.IsNullOrEmpty(collectionName))
                continue;

            var collection = database.GetCollection<BsonDocument>(collectionName);

            var propertiesToIndex = docType
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(MongoIndexAttribute)));

            foreach (var prop in propertiesToIndex)
            {
                var attribute = prop.GetCustomAttribute<MongoIndexAttribute>();
                var bsonElement =
                    prop.GetCustomAttribute<MongoDB.Bson.Serialization.Attributes.BsonElementAttribute>();
                var fieldName = bsonElement?.ElementName ?? prop.Name;

                try
                {
                    var indexKeys = attribute is { Descending: true }
                        ? Builders<BsonDocument>.IndexKeys.Descending(fieldName)
                        : Builders<BsonDocument>.IndexKeys.Ascending(fieldName);

                    var indexOptions = new CreateIndexOptions { Unique = attribute?.Unique };
                    var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, indexOptions);

                    await collection.Indexes.CreateOneAsync(indexModel);
                    Console.WriteLine(
                        $"--> [Mongo] Index garantido em '{collectionName}': {fieldName}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"--> [Mongo] Erro no index '{collectionName}': {ex.Message}"
                    );
                }
            }
        }
    }
}
