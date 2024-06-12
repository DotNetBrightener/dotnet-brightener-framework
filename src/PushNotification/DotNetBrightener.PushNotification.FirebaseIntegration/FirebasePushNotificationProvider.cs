using System.Text;
using DotNetBrightener.PushNotification.FirebaseIntegration.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DotNetBrightener.PushNotification.FirebaseIntegration;

public class FirebasePushNotificationProvider(
    IOptions<PushNotificationFirebaseSettings> firebaseSetting,
    ILogger<FirebasePushNotificationProvider>  logger)
    : IPushNotificationProvider
{
    public string PushNotificationType => PushNotificationEndpointType.FirebaseCloudMessaging;

    private readonly ILogger                          _logger          = logger;
    private readonly PushNotificationFirebaseSettings _firebaseSetting = firebaseSetting.Value;

    public async Task DeliverNotification(string[] deviceTokens, PushNotificationPayload payload)
    {
        var accessToken = await GetAccessToken(_firebaseSetting.ServiceKey);

        await deviceTokens.ParallelForEachAsync(async (deviceToken) =>
        {
            await DeliverMessagePayloadToDevice(payload, accessToken, deviceToken);
        });
    }

    private async Task DeliverMessagePayloadToDevice(PushNotificationPayload payload, string accessToken, string deviceToken)
    {
        var messageInformation = new PushNotificationMessageModel
        {
            notification = new Notification
            {
                title = payload.Title,
                body  = payload.Body
            },
            data  = payload.Data,
            token = deviceToken
        };

        PushNotificationMessageWrapper messageInformationWrapper = new()
        {
            MessageModel = messageInformation
        };

        var jsonMessage                  = JsonConvert.SerializeObject(messageInformationWrapper);
        var projectId                    = _firebaseSetting.ProjectId;
        var fireBasePushNotificationsUrl = new Uri($"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send");

        var request = new HttpRequestMessage(HttpMethod.Post, fireBasePushNotificationsUrl);

        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

        request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

        using (var client = new HttpClient())
        {
            try
            {
                _logger.LogInformation($"Sending request to Firebase CM: {jsonMessage}");
                var response     = await client.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Request sent successfully to Firebase CM. Response from Firebase CM: {responseText}");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while delivering message through Firebase CM");
            }
        }
    }

    private async Task<string> GetAccessToken(string serviceKeyJson)
    {
        var scopes = "https://www.googleapis.com/auth/firebase.messaging";
        var accessToken = await GoogleCredential.FromJson(serviceKeyJson)
                                                .CreateScoped(scopes)
                                                .UnderlyingCredential
                                                .GetAccessTokenForRequestAsync();

        return accessToken;
    }
}