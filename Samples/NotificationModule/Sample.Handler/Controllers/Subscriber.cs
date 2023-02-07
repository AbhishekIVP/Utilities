using ivp.edm.notification;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Handler.Controllers
{
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly ILogger<SubscriberController> _logger;
        private readonly CommandHandler _commandHandler;
        public SubscriberController(ILogger<SubscriberController> logger, CommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost]
        [Route("ProcessCommand")]
        public async Task ProcessCommand(Command command)
        {
            //TODO: Store this information to a Database
            //TODO: get the tenant information
            //TODO: get the user and groups from the AudienceGroupName 
            await _commandHandler.Execute(command, new string[] { "spanhotra@ivp.in" });
        }
    }
}