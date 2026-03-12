using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using NotificationService.Entity;
using NotificationService.Options;
using NotificationService.Services;
using NotificationService.Types;

namespace NotificationService.Providers;

public class EmailNotifyServiceProvider(
    INotificationMessageQueueDataService notificationMessageQueueDataService,
    ILogger<EmailNotifyServiceProvider>  logger,
    IOptions<EmailSmtpSetting>           emailSmtpSetting,
    IDateTimeProvider                    dateTimeProvider,
    IOptions<EmailNotificationSettings>  emailNotificationSettings)
    : BaseNotifyServiceProvider<EmailNotificationMessage>(
                                                          notificationMessageQueueDataService,
                                                          dateTimeProvider,
                                                          logger)
{
    private readonly EmailSmtpSetting          _emailSmtpSetting          = emailSmtpSetting.Value;
    private readonly EmailNotificationSettings _emailNotificationSettings = emailNotificationSettings.Value;

    protected override async Task DeliverSingleNotification(EmailNotificationMessage notificationMessage)
    {
        await DeliverEmails(_emailSmtpSetting,
                            smtpClient =>
                                DeliverNotification(notificationMessage, smtpClient)
                           );
    }

    protected override async Task DeliverManyNotifications(IEnumerable<EmailNotificationMessage> notificationMessages)
    {
        await Task.WhenAll(
                           notificationMessages.Select(notificationMessage =>
                                                           DeliverEmails(_emailSmtpSetting,
                                                                         async smtpClient =>
                                                                         {
                                                                             await
                                                                                 DeliverNotification(notificationMessage,
                                                                                                     smtpClient);
                                                                         })
                                                      )
                          );
    }

    private async Task DeliverNotification(EmailNotificationMessage notificationMessage,
                                           SmtpClient               smtpClient)
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
                            smtpClient,
                            from: _emailSmtpSetting.FromAddress,
                            displayName: _emailSmtpSetting.FromDisplayName,
                            cc: ccTargets,
                            bcc: bccTargets
                           );
        }
        catch (Exception exception)
        {
            errorMessage = exception.GetFullExceptionMessage();
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            await NotificationMessageQueueDataService.UpdateOne(q => q.Id == notificationMessage.Id,
                                                                _ => new NotificationMessageQueue
                                                                {
                                                                    LastAttemptUtc = DateTimeProvider.UtcNowWithOffset,
                                                                    LastAttemptException = errorMessage
                                                                });

            return;
        }

        await NotificationMessageQueueDataService.UpdateOne(q => q.Id == notificationMessage.Id,
                                                            _ => new NotificationMessageQueue
                                                            {
                                                                SentAtUtc = DateTimeProvider.UtcNowWithOffset,
                                                                SenderEntityId = notificationMessage
                                                                   .SenderEntityId
                                                            });

    }

    private static async Task SendEmail(IEnumerable<string> to,
                                        string              subject,
                                        string              body,
                                        SmtpClient          smtpClient,
                                        bool                isHtml      = true,
                                        IEnumerable<string> cc          = null,
                                        IEnumerable<string> bcc         = null,
                                        string              from        = "",
                                        string              displayName = "",
                                        string              replyTo     = "")
    {
        var mail = new MimeMessage
        {
            Subject = subject,
            Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = body
            }
        };

        mail.From.Add(new MailboxAddress(displayName, from));

        var mailsReplyTo = new List<string>();

        if (!string.IsNullOrEmpty(replyTo))
        {
            mailsReplyTo.AddRange(replyTo.Split([
                                                    ",", ";"
                                                ],
                                                StringSplitOptions.RemoveEmptyEntries));
        }

        foreach (var mailAdd in mailsReplyTo)
        {
            try
            {
                mail.ReplyTo.Add(new MailboxAddress(mailAdd, mailAdd));
            }
            catch
            {
            }
        }

        if (cc != null &&
            cc.Any())
        {
            mail.Cc.Clear();

            foreach (var s in cc)
            {
                try
                {
                    mail.Cc.Add(new MailboxAddress(s, s));
                }
                catch
                {
                }
            }
        }

        if (bcc != null &&
            bcc.Any())
        {
            mail.Bcc.Clear();

            foreach (var s in bcc)
            {
                try
                {
                    mail.Bcc.Add(new MailboxAddress(s, s));
                }
                catch
                {
                }
            }
        }

        var errors = new Dictionary<string, Exception>();

        foreach (var s in to)
        {
            try
            {
                mail.To.Add(new MailboxAddress(s, s));
                await smtpClient.SendAsync(mail);
                mail.To.Clear();
            }
            catch (Exception exception)
            {
                errors.Add(s, exception);
            }
        }

        if (errors.Any())
        {
            throw new AggregateException(errors.Select((p => p.Value)));
        }
    }

    private async Task DeliverEmails(EmailSmtpSetting       emailAccount,
                                     Func<SmtpClient, Task> deliverAction)
    {
        using (var smtpClient = new SmtpClient())
        {
            smtpClient.Timeout    = (emailAccount.Timeout ?? 120) * 1000;
            smtpClient.RequireTLS = emailAccount.EnableSsl;

            try
            {

                await smtpClient.ConnectAsync(emailAccount.Host, emailAccount.Port);
                await smtpClient.AuthenticateAsync(emailAccount.User, emailAccount.Password);

                await deliverAction.Invoke(smtpClient);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error while trying to deliver email");
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}