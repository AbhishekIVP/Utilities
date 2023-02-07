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