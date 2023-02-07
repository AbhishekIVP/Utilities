using FluentEmail.Core;
using ivp.edm.notification;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Handler.Controllers
{
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly ILogger<SubscriberController> _logger;
        public SubscriberController(IFluentEmail fluentEmail, ILogger<SubscriberController> logger)
        {
            _fluentEmail = fluentEmail;
            _logger = logger;
        }
        [HttpPost]
        [Route("ProcessCommand")]
        public async Task ProcessCommand(Command command)
        {
            // get the tenant information
            // get the user and groups from the AudienceGroupName 
            // get Type from Template Name => initialize adapter => send data
            Console.WriteLine("Subscriber received : " + command.Data?.Body);
            if (command.Template.Type == TemplateType.EMAIL)
            {
                var _response = await _fluentEmail.To("spanhotra@ivp.in").Body(command.Data?.Body).SendAsync();
                if (_response.Successful == false)
                {
                    string _errorMessage = string.Join(",", _response.ErrorMessages);
                    _logger.LogError(_errorMessage);
                    throw new Exception(_errorMessage);
                }
                else
                {
                    _logger.LogInformation($"Email sent with Id:{_response.MessageId} with Template {command.Template.Name} to audience {command.AudienceGroupName} raised by event {command.@Event.Name} from source {command.@Event.Source}");
                }
            }
        }
    }
}