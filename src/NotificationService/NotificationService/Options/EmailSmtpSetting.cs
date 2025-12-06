namespace NotificationService.Options;

public class EmailSmtpSetting
{
    public string Host { get; set; }

    public int Port { get; set; }

    public bool EnableSsl { get; set; }

    public int? Timeout { get; set; }

    public string User { get; set; }

    public string Password { get; set; }

    public string FromAddress { get; set; }

    public string FromDisplayName { get; set; }

    public string ReplyTo { get; set; }
}