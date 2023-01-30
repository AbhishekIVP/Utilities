using ivp.edm.apm;
using OpenTelemetrySample.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddMonitoring();
builder.Host.AddLogging();

//TODO: This does not look right, to pass Configuration and Environment like this
//builder.Services.AddAPM(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SampleMeters>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseOpenTelemetry();

app.Run();
