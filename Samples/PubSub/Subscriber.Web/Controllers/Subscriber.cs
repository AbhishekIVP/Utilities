using System.Text.Json.Serialization;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace Samples.PubSub.Subscriber.Web.Controllers
{
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        [HttpPost]
        [Route("ProcessMessage")]
        public IResult ProcessMessage([FromBody] Order order)
        {
            Console.WriteLine("Subscriber received : " + order);
            return Results.Ok(order);
        }
    }
    public record Order([property: JsonPropertyName("orderId")] int OrderId);
}