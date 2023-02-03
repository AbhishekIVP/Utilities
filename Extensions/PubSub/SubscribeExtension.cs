using ivp.edm.validations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ivp.edm.pubsub
{
    public static class SubscribeExtension
    {
        public static WebApplication StartSubscriber(this WebApplication app)
        {
            return StartSubscriber(app, _ => { });
        }
        public static WebApplication StartSubscriber(this WebApplication app, Action<List<TopicRouteMapping>> configureTopicRouteMappings)
        {
            ArgumentGuard.NotNull(app);

            app.UseCloudEvents();

            // needed for Dapr pub/sub routing
            app.MapSubscribeHandler();

            if (app.Environment.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            PubSubOptions _options = app.Services.GetRequiredService<IOptions<PubSubOptions>>().Value;

            //TODO:Multiple Clients
            List<string> _clients = new List<string>() { "client1", "client2", "client3" };
            //Adding Programmatic way
            configureTopicRouteMappings.Invoke(_options.TopicRouteMappings);
            foreach (TopicRouteMapping map in _options.TopicRouteMappings)
            {
                foreach (var queueName in map.QueueName)
                {
                    if (_options.IsMultiTenant)
                    {
                        foreach (string _client in _clients)
                        {
                            app.MapControllerRoute(
                                name: map.MethodRoute,
                                pattern: map.MethodRoute).WithTopic($"{_options.Name}-{_client}", queueName);
                        }
                    }
                    else
                    {
                        app.MapControllerRoute(
                                name: map.MethodRoute,
                                pattern: map.MethodRoute).WithTopic($"{_options.Name}", queueName);
                    }
                }
            }
            return app;
        }
    }
}