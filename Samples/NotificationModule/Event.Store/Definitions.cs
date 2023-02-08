namespace ivp.edm.notification;

public class EventStore
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public class TemplateStore
{
    public string Name { get; set; } = string.Empty;
    public TemplateType Type { get; set; } = TemplateType.EMAIL;
    public string JsonTemplate { get; set; } = string.Empty;
}

public enum TemplateType
{
    EMAIL,
    SLACK,
    BROWSER,

}

public class Audience
{
    public string AudienceGroupName { get; set; } = string.Empty;
    public string[]? GroupName { get; set; }
    public string[]? UserName { get; set; }
}

public record CommandData
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Attachment { get; set; } = string.Empty; //attachment location
    public Dictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
}

public class Command
{
    public Guid EventInstanceID { get; set; } = new Guid();
    public EventStore @Event { get; set; } = new EventStore();
    public List<string> TemplateNames { get; set; } = new List<string>();
    public CommandData? Data { get; set; }
    public string AudienceGroupName { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public bool IsCritical { get; set; } = true;
}