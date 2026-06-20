using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.Mongo;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class IdentityServicesExtensions
{
    public static WebApplicationBuilder AddIdentityPersistence(this WebApplicationBuilder builder)
    {
        var mongoConnString = builder.Configuration.GetConnectionString("MongoConnection");
        var dbName = builder.Configuration["MONGO_DATABASE_NAME"] ?? "GregCompanyMongo";

        var mongoUrl = new MongoDB.Driver.MongoUrlBuilder(mongoConnString)
        {
            DatabaseName = dbName
        }.ToMongoUrl().ToString();

        // Configuração nativa do ASP.NET Core Identity adaptada para MongoDB
        builder.Services.AddIdentityMongoDbProvider<Users, Roles, Guid>(
            mongoOptions =>
            {
                mongoOptions.ConnectionString = mongoUrl;
            }
        );

        builder.Services.Configure<IdentityOptions>(identityOptions =>
        {
            identityOptions.SignIn.RequireConfirmedAccount = true;
            identityOptions.Password.RequireDigit = false;
            identityOptions.Password.RequiredLength = 6;
        });

        return builder;
    }
}