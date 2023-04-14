using System.IdentityModel.Tokens.Jwt;
using System.Net;
using GrpcPerson;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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

        builder.Services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
        });
        builder.Services.AddGrpcReflection();
        builder.Services.AddOptions();
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.WebHost.UseKestrel(options =>
        {
            var ports = GetDefinedPorts(builder.Configuration);
            options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });

            options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });

        });
        builder.WebHost.CaptureStartupErrors(false);

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

        app.MapGrpcService<PersonService>();
        app.MapDefaultControllerRoute();
        app.MapControllers();
        app.MapGrpcReflectionService();
        app.MapGet("/_proto/", async ctx =>
        {
            ctx.Response.ContentType = "text/plain";
            using var fs = new FileStream(Path.Combine(app.Environment.ContentRootPath, "Proto", "basket.proto"), FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);
            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync();
                if (line != "/* >>" || line != "<< */")
                {
                    await ctx.Response.WriteAsync(line);
                }
            }
        });
        app.Run();
    }

    static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
    {
        var grpcPort = config.GetValue("GRPC_PORT", 5001);
        var port = config.GetValue("PORT", 80);
        return (port, grpcPort);
    }
}