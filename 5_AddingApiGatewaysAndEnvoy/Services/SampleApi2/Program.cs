using MassTransit;
using Sample.Contracts;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(x =>
{
    x.SuppressAsyncSuffixInActionNames = false;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();

//By connecting here we are making sure that our service
//cannot start until redis is ready. This might slow down startup,
//but given that there is a delay on resolving the ip address
//and then creating the connection it seems reasonable to move
//that cost to startup instead of having the first request pay the
//penalty.
builder.Services.AddSingleton<ConnectionMultiplexer>(sp =>
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