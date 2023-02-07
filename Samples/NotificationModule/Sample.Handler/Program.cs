using FluentEmail.MailKitSmtp;
using ivp.edm;
using ivp.edm.pubsub;
using ivp.edm.secrets;
using ivp.edm.validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddExtensions(Enable.PUB_SUB | Enable.SECRETS);

var _secrets = builder.Services.ValidatedInstance<SecretsManager>();

var _notificationOptions = new NotificationOptions();
builder.Configuration.GetSection("Notification").Bind(_notificationOptions);

if (string.IsNullOrEmpty(_notificationOptions.Password.ValueFrom) == false)
    ((SmtpClientOptions)_notificationOptions).Password = await _secrets.GetDefaultStoreSecretAsync(_notificationOptions.Password.ValueFrom);
else if (string.IsNullOrEmpty(_notificationOptions.Password.Value) == false)
    ((SmtpClientOptions)_notificationOptions).Password = _notificationOptions.Password.Value;

builder.Services.AddFluentEmail(_notificationOptions.FromEmail)
        .AddRazorRenderer()
        .AddMailKitSender(_notificationOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.StartSubscriber();

app.MapControllers();

app.Run();


public class NotificationOptions : SmtpClientOptions
{
    public new PasswordOptions Password { get; set; } = new PasswordOptions();
    public string FromEmail { get; set; } = "notifications@ivp.in";
}

public class PasswordOptions
{
    public string Value { get; set; } = string.Empty;
    public string ValueFrom { get; set; } = string.Empty;
}