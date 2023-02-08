using ivp.edm.notification;
using ivp.edm.notification.template;
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
        ICustomNotify? _client = null;
        Type _type = typeof(ICustomNotify);

        //TODO:get the Implementation from command.TemplateNames, execute them one by one
        foreach (var template in command.TemplateNames)
        {
            //TODO: get Type from Template Name => initialize adapter => send data
            TemplateStore _ = new TemplateStore() { Name = template };
            switch (_.Type)
            {
                case TemplateType.EMAIL:
                    _client = _serviceProvider.GetService<Email>();
                    _type = typeof(Email);
                    break;
                case TemplateType.SLACK:
                    _client = _serviceProvider.GetService<SlackPost>();
                    _type = typeof(SlackPost);
                    break;
            }
            ServiceNotFoundException.ThrowIfNull(_client, _type);
            await _client.Notify(command, template);
        }
    }
}