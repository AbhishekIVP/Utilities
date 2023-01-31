using Dapr.Client;
using ivp.edm.apm;
using ivp.edm.distributedlock;
using ivp.edm.redis;
using ivp.edm.secrets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());
builder.Services.AddScoped<SecretsManager>();

builder.Host.AddRedisClient();
builder.Host.AddDynamicLocking();
builder.Host.AddMonitoring();
builder.Host.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseOpenTelemetry();

app.Run();
