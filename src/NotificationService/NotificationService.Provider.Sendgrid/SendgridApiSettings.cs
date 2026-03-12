namespace NotificationService.Provider.Sendgrid;

/// <summary>
///     Configuration settings for SendGrid Web API integration
/// </summary>
public class SendgridApiSettings
{
    /// <summary>
    ///     The SendGrid API Key for authentication.
    ///     Generate this from your SendGrid account dashboard.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    ///     The sender email address.
    ///     This must be a verified sender in your SendGrid account.
    /// </summary>
    public string FromAddress { get; set; }

    /// <summary>
    ///     The sender display name shown to email recipients.
    /// </summary>
    public string FromDisplayName { get; set; }

    /// <summary>
    ///     Optional reply-to email address.
    ///     If not specified, replies will go to the FromAddress.
    /// </summary>
    public string ReplyTo { get; set; }
}

