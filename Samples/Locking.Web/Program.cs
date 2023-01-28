using System.Collections.Immutable;
using Medallion.Threading;
using Medallion.Threading.Redis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigurationOptions _options = new ConfigurationOptions()
{
    Password = builder.Configuration["DistributedLocking:Redis:Password:Value"],
    DefaultDatabase = Convert.ToInt32(builder.Configuration["DistributedLocking:Redis:Database"])
};
_options.EndPoints.Add(builder.Configuration["DistributedLocking:Redis:Endpoint"]);
var connection = await ConnectionMultiplexer.ConnectAsync(_options); // uses StackExchange.Redis

builder.Services.AddSingleton<IDistributedLockProvider>(_ => new RedisDistributedSynchronizationProvider(connection.GetDatabase()));

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

app.Run();
