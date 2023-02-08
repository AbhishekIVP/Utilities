using ivp.edm.secrets;
using ivp.edm.validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackAPI;

namespace ivp.edm.notification.template.implementation;

public class SlackPost : ICustomNotify
{
    private readonly ILogger<SlackPost> _logger;
    private readonly IConfiguration _configuration;
    private readonly SecretsManager _secretsManager;
    public SlackPost(ILogger<SlackPost> logger, IConfiguration configuration, SecretsManager secretsManager)
    {
        _logger = logger;
        _configuration = configuration;
        _secretsManager = secretsManager;
    }
    public async Task Notify(Command command, string templateName)
    {
        string? token = null;
        var _slackOptions = new SlackOptions();
        _configuration.GetSection("Notification:Slack").Bind(_slackOptions);

        if (string.IsNullOrEmpty(_slackOptions.Token.ValueFrom) == false)
            token = _secretsManager.GetDefaultStoreSecretAsync(_slackOptions.Token.ValueFrom).Result;
        else if (string.IsNullOrEmpty(_slackOptions.Token.Value) == false)
            token = _slackOptions.Token.Value;

        ArgumentGuard.NotNull(token);
        if (command.Data == null)
        {
            _logger.LogWarning("Command Data is null, not proceeding to send out a Slack!!!");
            return;
        }
        string? fullFilePath = command.Data.Attachment;
        string? channel = command.Data.Extras.ContainsKey("channel") ? command.Data.Extras["channel"] : command.Data.Subject;

        //TODO: get user information from command.AudienceGroupName
        string toUserName = "spanhotra@ivp.in";

        string message = string.Empty;
        if (command.Data.Body == null)
        {
            _logger.LogWarning("Command Data Body is null, not proceeding to send out a Slack!!!");
            return;
        }
        message = command.Data.Body;
        string _errorMessageChannel = string.Empty;
        string _errorMessageDM = string.Empty;

        await Task.Run(() =>
        {
            SlackClient client = new SlackClient(token);
            ManualResetEventSlim manualResetEventSlimChannel = new ManualResetEventSlim(false);
            ManualResetEventSlim manualResetEventSlimDM = new ManualResetEventSlim(false);

            if (string.IsNullOrEmpty(channel) == false)
            {
                client.GetChannelList((_) =>
                {
                    if (_.ok)
                    {
                        _logger.LogDebug("got channels");
                        Channel? _channel = _.channels.FirstOrDefault(_c => _c.name == channel);

                        if (_channel == null)
                        {
                            _errorMessageChannel = string.Format("Channel : '{0}' is not found.", channel);
                            manualResetEventSlimChannel.Set();
                        }
                        else
                        {
                            client.PostMessage(_m =>
                            {
                                if (_m.ok)
                                    _logger.LogInformation("message sent");
                                else
                                    _errorMessageChannel = string.Format("Channel : '{0}' found but could not send the message, error: {1}.", channel, _m.error);

                                if (string.IsNullOrEmpty(fullFilePath) == false && System.IO.File.Exists(fullFilePath))
                                {
                                    try
                                    {
                                        client.UploadFile(_mp =>
                                        {
                                            if (_mp.ok)
                                                _logger.LogInformation("message sent");
                                            else
                                                _errorMessageChannel = string.Format("Channel : '{0}' found but could not upload the file, error: {1}.", channel, _m.error);
                                            manualResetEventSlimChannel.Set();
                                        }, System.IO.File.ReadAllBytes(fullFilePath), Path.GetFileName(fullFilePath), new string[] { _channel.id });
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"Error while uploading the file {fullFilePath} with exception {ex}");
                                        _errorMessageChannel = $"Error while uploading the file {fullFilePath} with exception {ex}";
                                        manualResetEventSlimChannel.Set();
                                    }
                                }
                                else
                                    manualResetEventSlimChannel.Set();
                            }, _channel.id, message);
                        }
                    }
                    else
                    {
                        _errorMessageChannel = string.Format("Channel : '{0}' Request failed with error: {1}.", channel, _.error);
                        manualResetEventSlimChannel.Set();
                    }
                });
            }
            else
                manualResetEventSlimChannel.Set();

            if (string.IsNullOrEmpty(toUserName) == false)
            {
                client.GetUserList(_ =>
                {
                    if (_.ok)
                    {
                        _logger.LogDebug("I got users");
                        User? _user = _.members.FirstOrDefault(_c => _c.name == toUserName);
                        if (_user == null)
                        {
                            _errorMessageDM = string.Format("User : '{0}' is not found.", toUserName);
                            manualResetEventSlimDM.Set();
                        }
                        else
                        {
                            client.GetDirectMessageList(_dm =>
                            {
                                if (_dm.ok)
                                {
                                    if (_dm.ims == null)
                                    {
                                        _errorMessageDM = string.Format("User : '{0}' Direct Messages are not found.", toUserName);
                                        manualResetEventSlimDM.Set();
                                    }
                                    else
                                    {
                                        var _dmchannel = _dm.ims.FirstOrDefault(x => x.user.Equals(_user.id));
                                        if (_dmchannel == null)
                                        {
                                            _errorMessageDM = string.Format("User : '{0}' Direct Messages (total {1}) are not found.", toUserName, _dm.ims.Length);
                                            manualResetEventSlimDM.Set();
                                        }
                                        else
                                        {
                                            client.PostMessage(_m =>
                                            {
                                                if (_m.ok)
                                                    _logger.LogDebug("message sent successfully");
                                                else
                                                    _errorMessageDM = string.Format("Channel : '{0}' found but could not send the message, error: {1}.", _dmchannel.user, _m.error);
                                                manualResetEventSlimDM.Set();
                                            }, _dmchannel.id, message);
                                        }
                                    }
                                }
                                else
                                {
                                    _errorMessageDM = string.Format("User : '{0}' DM Request failed with error: {1}.", toUserName, _dm.error);
                                    manualResetEventSlimDM.Set();
                                }
                            });
                        }
                    }
                    else
                    {
                        _errorMessageDM = string.Format("User : '{0}' Request failed with error: {1}.", toUserName, _.error);
                        manualResetEventSlimDM.Set();
                    }
                });
            }
            else
                manualResetEventSlimDM.Set();

            manualResetEventSlimChannel.Wait();
            manualResetEventSlimDM.Wait();
        });

        if (string.IsNullOrEmpty(_errorMessageChannel) == false || string.IsNullOrEmpty(_errorMessageDM) == false)
        {
            string _errorMessage = string.Format("Channel error : {0}, DM error : {1}", string.IsNullOrEmpty(_errorMessageChannel) ? "NO ERROR" : _errorMessageChannel,
                string.IsNullOrEmpty(_errorMessageDM) ? "NO ERROR" : _errorMessageDM);
            throw new Exception(_errorMessage);
        }

        _logger.LogDebug("End");
    }

    internal class SlackOptions
    {
        public PasswordOptions Token { get; set; } = new PasswordOptions();
    }
}