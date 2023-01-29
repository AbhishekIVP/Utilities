using ivp.edm.apm;
using ivp.edm.distributedlock;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConnectionMultiplexer? _redisConnectionForLocking = null;

builder.Host.AddDynamicRedisLocking(_ => _redisConnectionForLocking = _);
builder.Host.AddMonitoring(_redisConnectionForLocking);
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
