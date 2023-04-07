using MassTransit;
using Microsoft.OpenApi.Models;
using Sample.Contracts;
using SampleApi2.Infrastructure.Middlewares;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

internal class Program
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

        services.AddAuthentication("Bearer").AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "sampleapi2";
            options.TokenValidationParameters.ValidateAudience = false;
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "sampleapi2");
            });
        });
    }

    private static void AddSwaggerAuthorization(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sample API 2",
                Version = "v1",
                Description = "Sample HTTP API 2"
            });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrl")}/connect/authorize"),
                        TokenUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrl")}/connect/token"),
                        Scopes = new Dictionary<string, string>()
                                {
                                    { "sampleapi2", "Sample API 2" }
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
        

        builder.Services.AddSingleton<IProductRepository, ProductRepository>();

        //By connecting here we are making sure that our service
        //cannot start until redis is ready. This might slow down startup,
        //but given that there is a delay on resolving the ip address
        //and then creating the connection it seems reasonable to move
        //that cost to startup instead of having the first request pay the
        //penalty.
        builder.Services.AddSingleton(sp =>
        {
            var configuration = ConfigurationOptions.Parse(builder.Configuration["ConnectionString"], true);

            return ConnectionMultiplexer.Connect(configuration);
        });

        builder.Services.AddMassTransit(mt =>
        {
            mt.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration["EventBusConnection"];
                cfg.Host(connectionString);
            });

            mt.AddRequestClient<SubmitOrder>(new Uri("exchange:customer-order"));
        });

        builder.Services.AddOptions();

        var app = builder.Build();

        var pathBase = app.Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseSwagger()
        .UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint($"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json", "SampleApi2 V1");
            setup.OAuthClientId("sampleapi2swaggerui");
            setup.OAuthAppName("Sample Api2 Swagger UI");
        });

        app.UseRouting();
        app.UseCors("CorsPolicy");

        ConfigureAuthenticationAndAuthorization(app);

        app.MapDefaultControllerRoute();
        app.MapControllers();

        app.Run();
    }

}