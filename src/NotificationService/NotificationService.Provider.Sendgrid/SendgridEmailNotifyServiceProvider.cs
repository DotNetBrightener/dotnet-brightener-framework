using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Entity;
using NotificationService.Options;
using NotificationService.Providers;
using NotificationService.Services;
using NotificationService.Types;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationService.Provider.Sendgrid;

public class SendgridEmailNotifyServiceProvider(
    INotificationMessageQueueDataService        notificationMessageQueueDataService,
    ILogger<SendgridEmailNotifyServiceProvider> logger,
    IOptions<SendgridApiSettings>               sendgridApiSettings,
    IDateTimeProvider                           dateTimeProvider,
    IOptions<EmailNotificationSettings>         emailNotificationSettings)
    : BaseNotifyServiceProvider<EmailNotificationMessage>(
                                                          notificationMessageQueueDataService,
                                                          dateTimeProvider,
                                                          logger)
{
    private readonly SendgridApiSettings        _sendgridApiSettings       = sendgridApiSettings.Value;
    private readonly EmailNotificationSettings  _emailNotificationSettings = emailNotificationSettings.Value;

    protected override async Task DeliverSingleNotification(EmailNotificationMessage notificationMessage)
    {
        await DeliverNotification(notificationMessage);
    }

    protected override async Task DeliverManyNotifications(IEnumerable<EmailNotificationMessage> notificationMessages)
    {
        await Task.WhenAll(notificationMessages.Select(DeliverNotification));
    }

    private async Task DeliverNotification(EmailNotificationMessage notificationMessage)
    {
        var now = DateTimeProvider.UtcNowWithOffset;

        // the message is not for sending now, skip it
        if (notificationMessage.PlanToSendAtUtc.HasValue &&
            notificationMessage.PlanToSendAtUtc > now)
        {
            return;
        }

        // within 3 minutes if the message was being attempted to be sent, skip it
        if (notificationMessage.LastAttemptUtc.HasValue &&
            now.AddMinutes(-3) < notificationMessage.LastAttemptUtc)
        {
            return;
        }

        // mark the message is being sent
        await NotificationMessageQueueDataService.UpdateOne(q => q.Id == notificationMessage.Id,
                                                            _ => new NotificationMessageQueue
                                                            {
                                                                LastAttemptUtc = now
                                                            });
        var errorMessage = string.Empty;

        try
        {
            var deliveryTarget = new List<string>
            {
                notificationMessage.DeliveryTarget
            };

            var ccTargets = notificationMessage.CcTargets.Split([
                                                                    ",", ";"
                                                                ],
                                                                StringSplitOptions.RemoveEmptyEntries)
                                               .ToList();

            var bccTargets = notificationMessage.BccTargets.Split([
                                                                      ",", ";"
                                                                  ],
                                                                  StringSplitOptions.RemoveEmptyEntries)
                                                .ToList();

            if (_emailNotificationSettings.OverrideRecipients)
            {
                deliveryTarget.Clear();
                ccTargets.Clear();
                bccTargets.Clear();

                deliveryTarget.AddRange(_emailNotificationSettings.RecipientsList);
            }

            bccTargets.AddRange(_emailNotificationSettings.AlwaysBccList);

            await SendEmail(deliveryTarget,
                            notificationMessage.MessageTitle,
                            notificationMessage.MessageBody,
                            from: _sendgridApiSettings.FromAddress,
                            displayName: _sendgridApiSettings.FromDisplayName,
                            replyTo: _sendgridApiSettings.ReplyTo,
                            cc: ccTargets,
                            bcc: bccTargets);
        }
        catch (Exception exception)
        {
            errorMessage = exception.GetFullExceptionMessage();
            logger.LogError(exception, "Error while trying to deliver email via SendGrid API");
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            await NotificationMessageQueueDataService.UpdateOne(q => q.Id == notificationMessage.Id,
                                                                _ => new NotificationMessageQueue
                                                                {
                                                                    LastAttemptUtc       = DateTimeProvider.UtcNowWithOffset,
                                                                    LastAttemptException = errorMessage
                                                                });

            return;
        }

        await NotificationMessageQueueDataService.UpdateOne(q => q.Id == notificationMessage.Id,
                                                            _ => new NotificationMessageQueue
                                                            {
                                                                SentAtUtc      = DateTimeProvider.UtcNowWithOffset,
                                                                SenderEntityId = notificationMessage.SenderEntityId
                                                            });
    }

    private async Task SendEmail(IEnumerable<string> to,
                                 string              subject,
                                 string              body,
                                 bool                isHtml      = true,
                                 IEnumerable<string> cc          = null,
                                 IEnumerable<string> bcc         = null,
                                 string              from        = "",
                                 string              displayName = "",
                                 string              replyTo     = "")
    {
        var client = new SendGridClient(_sendgridApiSettings.ApiKey);
        var msg    = new SendGridMessage
        {
            From    = new EmailAddress(from, displayName),
            Subject = subject
        };

        // Add content based on type
        if (isHtml)
        {
            msg.AddContent(MimeType.Html, body);
        }
        else
        {
            msg.AddContent(MimeType.Text, body);
        }

        // Add To recipients
        var toList = to.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        foreach (var recipient in toList)
        {
            try
            {
                msg.AddTo(new EmailAddress(recipient.Trim()));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to add recipient: {Recipient}", recipient);
            }
        }

        // Add CC recipients
        if (cc != null)
        {
            foreach (var ccEmail in cc.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                try
                {
                    msg.AddCc(new EmailAddress(ccEmail.Trim()));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to add CC recipient: {CcEmail}", ccEmail);
                }
            }
        }

        // Add BCC recipients
        if (bcc != null)
        {
            foreach (var bccEmail in bcc.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                try
                {
                    msg.AddBcc(new EmailAddress(bccEmail.Trim()));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to add BCC recipient: {BccEmail}", bccEmail);
                }
            }
        }

        // Add Reply-To if specified
        if (!string.IsNullOrEmpty(replyTo))
        {
            var replyToAddresses = replyTo.Split([",", ";"], StringSplitOptions.RemoveEmptyEntries);

            if (replyToAddresses.Length > 0)
            {
                // SendGrid only supports a single reply-to address
                msg.ReplyTo = new EmailAddress(replyToAddresses[0].Trim());
            }
        }

        var response = await client.SendEmailAsync(msg);

        if (response.StatusCode != HttpStatusCode.OK &&
            response.StatusCode != HttpStatusCode.Accepted)
        {
            var responseBody = await response.Body.ReadAsStringAsync();

            throw new SendGridApiException(
                $"SendGrid API returned status {(int)response.StatusCode} ({response.StatusCode}): {responseBody}",
                response.StatusCode,
                responseBody);
        }

        logger.LogDebug("Email sent successfully via SendGrid API. Status: {StatusCode}", response.StatusCode);
    }
}

/// <summary>
///     Exception thrown when SendGrid API returns an error response
/// </summary>
public class SendGridApiException(string message, HttpStatusCode statusCode, string responseBody)
    : Exception(message)
{
    public HttpStatusCode StatusCode   { get; } = statusCode;
    public string         ResponseBody { get; } = responseBody;
}