using Microsoft.EntityFrameworkCore;
using SampleApi1.Infrastructure;
using System.Reflection;

public static class CustomExtensions
{
    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var assemblyName = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        services
            .AddDbContext<PersonContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionString"],
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(assemblyName);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });

        return services;
    }
}