using ivp.edm;
using ivp.edm.pubsub;
using ivp.edm.notification.template.implementation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.AddExtensions(Enable.PUB_SUB | Enable.SECRETS | Enable.APM | Enable.LOG);

builder.Services.AddNotifications(builder.Configuration);

var app = builder.Build();

app.StartSubscriber(_ =>
{
    if (_.Exists(__ => __.MethodRoute == "ProcessCommand") == false)
        _.Add(new TopicRouteMapping() { MethodRoute = "ProcessCommand", QueueName = new List<string>() { "notificationqueue" } });
});

app.MapControllers();

app.Run();