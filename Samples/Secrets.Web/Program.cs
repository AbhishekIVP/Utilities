using ivp.edm;
using ivp.edm.apm;

var builder = WebApplication.CreateBuilder(args);

builder.AddExtensions(Enable.APM | Enable.LOG | Enable.DAPR | Enable.SECRETS);

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

app.MapGet("/", () =>
{
    return "Hello Worlds";
});

app.UseAuthorization();

app.MapControllers();
app.UseOpenTelemetry();

app.Run();
