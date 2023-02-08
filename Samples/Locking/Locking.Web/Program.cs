using ivp.edm;
using ivp.edm.apm;

var builder = WebApplication.CreateBuilder(args);

builder.AddExtensions();
// builder.AddExtensions(Enable.DAPR | Enable.APM);
// builder.AddExtensions(Enable.APM | Enable.DAPR | Enable.LOCKING | Enable.REDIS | Enable.SECRETS | Enable.LOG);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
