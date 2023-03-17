var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("sampleApi1", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SampleApi1"]);
});

builder.Services.AddHttpClient("sampleApi2", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SampleApi2"]);
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/GetSamples", async (IHttpClientFactory factory) =>
{
    HttpClient sampleApi1Client = factory.CreateClient("sampleApi1");
    HttpClient sampleApi2Client = factory.CreateClient("sampleApi2");

    string response = await sampleApi1Client.GetStringAsync("WeatherForecast");
    string response2 = await sampleApi2Client.GetStringAsync("WeatherForecast");

    return response;
});
app.Run();