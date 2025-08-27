using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotNetBrightener.PushNotification.APN.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotNetBrightener.PushNotification.APN;

public class ApnPushNotificationProvider(
    IOptions<ApnSettings> apnSettings,
    ILogger<ApnPushNotificationProvider> logger)
    : IPushNotificationProvider
{
    public string PushNotificationType => PushNotificationEndpointType.Ios;

    private readonly ILogger<ApnPushNotificationProvider> _logger = logger;
    private readonly ApnSettings _apnSettings = apnSettings.Value;

    public async Task DeliverNotification(string[] deviceTokens, PushNotificationPayload payload)
    {
        var jwtToken = GenerateJwtToken();

        await deviceTokens.ParallelForEachAsync(async (deviceToken) =>
        {
            await DeliverNotificationToDevice(deviceToken, payload, jwtToken);
        });
    }

    private async Task DeliverNotificationToDevice(string deviceToken, PushNotificationPayload payload, string jwtToken)
    {
        try
        {
            var apnPayload = CreateApnPayload(payload);
            var jsonPayload = JsonSerializer.Serialize(apnPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var url = $"{_apnSettings.ApnServerUrl}/3/device/{deviceToken}";
            
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            
            request.Headers.Add("authorization", $"bearer {jwtToken}");
            request.Headers.Add("apns-topic", _apnSettings.BundleId);
            request.Headers.Add("apns-push-type", "alert");
            request.Headers.Add("apns-priority", "10");
            
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending APN notification to device {DeviceToken}: {Payload}", 
                deviceToken, jsonPayload);

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("APN notification sent successfully to device {DeviceToken}. ApnsId: {ApnsId}", 
                    deviceToken, response.Headers.GetValues("apns-id").FirstOrDefault());
            }
            else
            {
                var apnResponse = JsonSerializer.Deserialize<ApnResponse>(responseContent);
                _logger.LogError("Failed to send APN notification to device {DeviceToken}. Status: {StatusCode}, Reason: {Reason}", 
                    deviceToken, response.StatusCode, apnResponse?.Reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending APN notification to device {DeviceToken}", deviceToken);
        }
    }

    private ApnPayload CreateApnPayload(PushNotificationPayload payload)
    {
        var apnPayload = new ApnPayload
        {
            Aps = new ApnAps
            {
                Alert = new ApnAlert
                {
                    Title = payload.Title,
                    Body = payload.Body
                },
                Sound = "default"
            },
            CustomData = payload.Data
        };

        return apnPayload;
    }

    private string GenerateJwtToken()
    {
        var now = DateTimeOffset.UtcNow;
        var iat = now.ToUnixTimeSeconds();
        var exp = now.AddHours(1).ToUnixTimeSeconds();

        var header = new JwtHeader();
        header["alg"] = "ES256";
        header["kid"] = _apnSettings.KeyId;

        var payload = new JwtPayload
        {
            { "iss", _apnSettings.TeamId },
            { "iat", iat },
            { "exp", exp }
        };

        var privateKeyBytes = Convert.FromBase64String(_apnSettings.PrivateKey);
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        var signingCredentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa), SecurityAlgorithms.EcdsaSha256);
        var token = new JwtSecurityToken(header, payload);
        
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
