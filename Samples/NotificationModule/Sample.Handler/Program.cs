using ivp.edm;
using ivp.edm.pubsub;
using ivp.edm.notification.template.implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddExtensions(Enable.PUB_SUB | Enable.SECRETS);

builder.Services.AddNotifications(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.StartSubscriber(_ =>
{
    if (_.Exists(__ => __.MethodRoute == "ProcessCommand") == false)
        _.Add(new TopicRouteMapping() { MethodRoute = "ProcessCommand", QueueName = new List<string>() { "notificationqueue" } });
});

app.MapControllers();

app.Run();