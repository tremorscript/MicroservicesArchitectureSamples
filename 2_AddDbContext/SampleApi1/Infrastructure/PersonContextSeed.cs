using Polly;
using Polly.Retry;
using SampleApi1.Models;
using System.Data.SqlClient;

namespace SampleApi1.Infrastructure;

public class PersonContextSeed
{
    public async Task SeedAsync(PersonContext context, IHostEnvironment env, ILogger<PersonContextSeed> logger)
    {
        var policy = CreatePolicy(logger, nameof(PersonContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            if (!context.Persons.Any())
            {
                await context.Persons.AddRangeAsync(GetPeople());
            }

            await context.SaveChangesAsync();
        });
    }

    private Person[] GetPeople()
    {
        var people = new[] {
            new Person { FirstName = "Test1", LastName = "Test2" },
            new Person { FirstName = "Test2", LastName = "Test3" },
            new Person { FirstName = "Test4", LastName = "Test5" },
            new Person { FirstName = "Test6", LastName = "Test7" },
            new Person { FirstName = "Test8", LastName = "Test9" },
            new Person { FirstName = "Test10", LastName = "Test11" },
            new Person { FirstName = "Test12", LastName = "Test13" },
            new Person { FirstName = "Test14", LastName = "Test15" },
            };
        return people;
    }

    private AsyncRetryPolicy CreatePolicy(ILogger<PersonContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>().
            WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", prefix, exception.GetType().Name, exception.Message, retry, retries);
                }
            );
    }
}