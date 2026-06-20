using MeuCrudCsharp.Features.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.Mongo;
using MongoDB.Driver;

namespace MeuCrudCsharp.Extensions.Services.Persistence;

public static class IdentityServicesExtensions
{
    public static WebApplicationBuilder AddIdentityPersistence(this WebApplicationBuilder builder)
    {
        var mongoConnString = builder.Configuration.GetConnectionString("MongoConnection");
        var dbName = builder.Configuration["MONGO_DATABASE_NAME"] ?? "GregCompanyMongo";

        var mongoUrl = new MongoUrlBuilder(mongoConnString)
        {
            DatabaseName = dbName
        }.ToMongoUrl().ToString();

        // ConfiguraÃ§Ã£o nativa do ASP.NET Core Identity adaptada para MongoDB
        builder.Services.AddIdentityMongoDbProvider<Users, Roles, string>(
            identityOptions => { },
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
