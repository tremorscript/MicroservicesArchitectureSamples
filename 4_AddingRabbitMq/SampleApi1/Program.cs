using MassTransit;
using SampleApi1.Components;
using SampleApi1.Infrastructure;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
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

        app.MigrateDbContext<PersonContext>((context, services) =>
        {
            var env = app.Services.GetService<IHostEnvironment>();
            var logger = services.GetService<ILogger<PersonContextSeed>>();
            new PersonContextSeed().SeedAsync(context, env, logger).Wait();
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}