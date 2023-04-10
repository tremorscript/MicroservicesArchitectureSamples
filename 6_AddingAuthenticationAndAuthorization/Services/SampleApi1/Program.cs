using System.IdentityModel.Tokens.Jwt;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SampleApi1.Components;
using SampleApi1.Infrastructure;
using SampleApi1.Infrastructure.Middlewares;

public class Program
{
    private static void ConfigureAuthenticationAndAuthorization(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    private static void AddAuthorization(IServiceCollection services, string identityUrl)
    {
        // prevent from mapping "sub" claim to nameidentifier.
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "sampleapi1";
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidateIssuer = false;
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "sampleapi1");
            });
        });
    }

    private static void AddSwaggerAuthorization(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sample API 1",
                Version = "v1",
                Description = "Sample HTTP API 1"
            });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                        TokenUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                                {
                                    { "sampleapi1", "Sample API 1" }
                                }
                    }
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });
    }
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers(x =>
        {
            x.SuppressAsyncSuffixInActionNames = false;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        AddSwaggerAuthorization(builder);

        var identityUrl = builder.Configuration["IdentityUrl"];
        AddAuthorization(builder.Services, identityUrl);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        builder.Services.AddCustomDbContext(builder.Configuration);
        builder.Services.AddMassTransit(mt =>
        {
            mt.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration["EventBusConnection"];
                cfg.Host(connectionString);

                cfg.ReceiveEndpoint("order-service", e =>
                {
                    e.Bind("customer-order");
                    e.Consumer<SubmitOrderConsumer>();
                });
            });
        });

        WebApplication app = builder.Build();

        var pathBase = app.Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.MigrateDbContext<PersonContext>((context, services) =>
        {
            var env = app.Services.GetService<IHostEnvironment>();
            var logger = services.GetService<ILogger<PersonContextSeed>>();
            new PersonContextSeed().SeedAsync(context, env, logger).Wait();
        });


        app.UseSwagger()
        .UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "SampleApi2 V1");
            setup.OAuthClientId("sampleapi1swaggerui");
            setup.OAuthClientSecret(string.Empty);
            setup.OAuthRealm(string.Empty);
            setup.OAuthAppName("Sample Api1 Swagger UI");
        });

        app.UseRouting();
        app.UseCors("CorsPolicy");

        ConfigureAuthenticationAndAuthorization(app);

        app.MapDefaultControllerRoute();
        app.MapControllers();

        app.Run();
    }
}