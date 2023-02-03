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

    [ApiController]
    public class Subscriber2Controller : ControllerBase
    {
        [HttpPost]
        [Route("ProcessMessage2")]
        public IResult ProcessMessage2([FromBody] Order order)
        {
            Console.WriteLine("Subscriber2 received : " + order);
            return Results.Ok(order);
        }
    }
    public record Order([property: JsonPropertyName("orderId")] int OrderId);
}