using ivp.edm.notification;
using ivp.edm.notification.template.implementation;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class CommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    public CommandHandler(ILogger<CommandHandler> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(Command command, string[] userids)
    {
        //TODO:get the Implementation from command.TemplateNames
        //TODO:execute them one by one
        foreach (var template in command.TemplateNames)
        {
            //TODO: get Type from Template Name => initialize adapter => send data
            TemplateStore _ = new TemplateStore() { Name = template };
            if (_.Type == TemplateType.EMAIL)
            {
                var _emailClient = _serviceProvider.GetService<Email>();
                ServiceNotFoundException.ThrowIfNull<Email>(_emailClient);
                await _emailClient.Notify(command, template);
            }
        }
    }
}