using Webhooks.API.Infrastructure;

public class Program
{
    private static void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }


    private static void Configure(IApplicationBuilder app, IConfiguration configuration)
    {
        var pathBase = configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }


        app.UseSwagger()
        .UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "SampleApi2 V1");
            setup.OAuthClientId("webhooksswaggerui");
            setup.OAuthClientSecret(string.Empty);
            setup.OAuthRealm(string.Empty);
            setup.OAuthAppName("Webhooks Api Swagger UI");
        });
        app.UseRouting();
        app.UseCors("CorsPolicy");
        ConfigureAuth(app);
        app.UseEndpoints(e =>
        {
            e.MapDefaultControllerRoute();
            e.MapControllers();
        });
    }

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var identityUrl = builder.Configuration["IdentityUrl"];
        builder.Services
                .AddCustomRouting(builder.Configuration)
                .AddCustomAuthentication(identityUrl)
                .AddCustomAuthorization(builder.Configuration)
                .AddSwagger(builder.Configuration)
                .AddCustomDbContext(builder.Configuration);

        var app = builder.Build();

        app.MigrateDbContext<WebhooksContext>((_, _) =>
        {
        });

        Configure(app, app.Configuration);

        app.Run();
    }
}