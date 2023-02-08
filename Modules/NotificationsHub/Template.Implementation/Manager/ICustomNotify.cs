namespace ivp.edm.notification.template;
public interface ICustomNotify
{
    Task Notify(Command command, string templateName);
}
