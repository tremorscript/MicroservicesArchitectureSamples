using GrpcPerson;
using Microsoft.Extensions.Options;
using Web.Bff.Sample.Controllers;
using Web.Bff.Sample.Services;
using Web.BffSample.Infrastructure;

namespace Web.Bff.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //One way to populate configuration to an object.
        builder.Services.Configure<UrlsConfig>(builder.Configuration.GetSection("urls"));

        //Named Client
        builder.Services.AddHttpClient<SampleApi1Controller>();

        //Grpc Services
        builder.Services.AddTransient<GrpcExceptionInterceptor>();
        builder.Services.AddScoped<IPersonService, PersonService>();
        builder.Services.AddGrpcClient<Person.PersonClient>((services, options) =>
        {
            var personApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.GrpcPerson;
            options.Address = new Uri(personApi);
        }).AddInterceptor<GrpcExceptionInterceptor>();

        var app = builder.Build();

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