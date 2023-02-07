using FluentEmail.Core;
using Microsoft.Extensions.Logging;

namespace ivp.edm.notification.template.implementation;

public class Email : ICustomNotify
{
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<Email> _logger;
    public Email(IFluentEmail fluentEmail, ILogger<Email> logger)
    {
        _fluentEmail = fluentEmail;
        _logger = logger;
    }

    public async Task Notify(Command command, string templateName)
    {
        // get Type from Template Name => initialize adapter => send data
        var _response = await _fluentEmail.To("spanhotra@ivp.in").Body(command.Data?.Body).SendAsync();
        if (_response.Successful == false)
        {
            string _errorMessage = string.Join(",", _response.ErrorMessages);
            _logger.LogError(_errorMessage);
            throw new Exception(_errorMessage);
        }
        else
        {
            _logger.LogInformation($"Email sent with Id:{_response.MessageId} with Template {templateName} to audience {command.AudienceGroupName} raised by event {command.@Event.Name} from source {command.@Event.Source}");
        }
    }
}

