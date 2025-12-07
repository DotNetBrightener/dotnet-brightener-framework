namespace NotificationService.Options;

public class EmailNotificationSettings
{
    public bool OverrideRecipients { get; set; } = false;

    public string FromAddress     { get; set; }

    public string AdminRecipients { get; set; }

    public string Recipients { get; set; } = string.Empty;

    public string AlwaysBcc { get; set; } = string.Empty;

    public string[] AdminRecipientsList =>
        AdminRecipients.Split([
                                  ";", ","
                              ],
                              StringSplitOptions.RemoveEmptyEntries);

    public string[] RecipientsList =>
        Recipients.Split([
                             ";", ","
                         ],
                         StringSplitOptions.RemoveEmptyEntries);

    public string[] AlwaysBccList =>
        AlwaysBcc.Split([
                            ";", ","
                        ],
                        StringSplitOptions.RemoveEmptyEntries);
}