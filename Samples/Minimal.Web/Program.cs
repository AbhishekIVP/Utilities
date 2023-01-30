using ivp.edm.apm;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddMonitoring();
builder.Host.AddLogging();

var app = builder.Build();

app.MapGet("/", () =>
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5124/weatherforecast");
    return client.GetStringAsync("").Result;
});

app.Run();
