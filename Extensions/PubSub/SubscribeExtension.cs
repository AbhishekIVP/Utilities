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

            //Adding Programmatic way
            configureTopicRouteMappings.Invoke(_options.TopicRouteMapping);
            foreach (TopicRouteMapping map in _options.TopicRouteMapping)
            {
                app.MapControllerRoute(
                    name: map.MethodRoute,
                    pattern: map.MethodRoute).WithTopic(_options.PubSubName, map.QueueName);
            }
            return app;
        }
    }
    public class TopicRouteMapping
    {
        public string QueueName { get; set; } = string.Empty;
        public string MethodRoute { get; set; } = string.Empty;
    }
}