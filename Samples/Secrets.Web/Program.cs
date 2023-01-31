using ivp.edm.apm;
using ivp.edm.secrets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprClient();
builder.Services.AddScoped<SecretsManager>();

builder.Host.AddMonitoring();
builder.Host.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () =>
{
    return "Hello Worlds";
});

app.UseAuthorization();

app.MapControllers();
app.UseOpenTelemetry();

app.Run();
