using Hangfire;
using MeuCrudCsharp.Extensions.App.Endpoints;
using MeuCrudCsharp.Extensions.App.Initialization;
using MeuCrudCsharp.Extensions.App.Middlewares;
using MeuCrudCsharp.Extensions.Services.McpServices;
using MeuCrudCsharp.Extensions.Services.Web;

namespace MeuCrudCsharp.Extensions.App;

public static class CentralAppExtensions
{
    public static async Task<WebApplication> ConfigureAppPipeline(this WebApplication app)
    {
        // 1. FASE DE INICIALIZAÇÃO (BOOTSTRAPPING)
        await app.InitializeSqlDatabaseAsync();
        await app.InitializeMongoIndexesAsync();

        // 2. FASE DE MIDDLEWARES (PIPELINE HTTP)
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

        app.UseRouting();
        app.UseCors(WebSecurityServicesExtensions.CorsPolicyName); // Certifique-se de importar o namespace correto para isso

        app.UseGregNetworkMonitoring();

        // Módulo de Autenticação/Autorização isolado
        app.UseAuthFeatures();

        app.UseHangfireDashboard();

        // 3. FASE DE MAPEAMENTO DE ENDPOINTS (ROTEAMENTO FINAL)
        app.MapApplicationHubs(); // Nossos Hubs Isolados
        app.MapRazorPages();
        app.MapControllers();
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        // Registro do Servidor de Contexto MCP
        app.UseMcpEndpoints();

        return app;
    }
}
