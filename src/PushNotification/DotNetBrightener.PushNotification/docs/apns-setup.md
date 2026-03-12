# Apple Push Notification Service (APNs) Setup Guide

This guide walks you through setting up Apple Push Notification service (APNs) for use with the DotNetBrightener Push Notification library.

## Prerequisites

- Apple Developer Account (paid membership required)
- iOS app with a valid Bundle ID
- Xcode (for iOS development and testing)

## Step 1: Create an App ID

1. Log in to the [Apple Developer Portal](https://developer.apple.com/account/)
2. Navigate to **Certificates, Identifiers & Profiles**
3. Click **Identifiers** in the sidebar
4. Click the **+** button to create a new identifier
5. Select **App IDs** and click **Continue**
6. Choose **App** and click **Continue**
7. Fill in the details:
   - **Description**: Your app name
   - **Bundle ID**: Use explicit Bundle ID (e.g., `com.yourcompany.yourapp`)
8. Under **Capabilities**, check **Push Notifications**
9. Click **Continue** and then **Register**

## Step 2: Create an APNs Key

### Option A: APNs Authentication Key (Recommended)

1. In the Apple Developer Portal, go to **Keys**
2. Click the **+** button to create a new key
3. Enter a **Key Name** (e.g., "Push Notification Key")
4. Check **Apple Push Notifications service (APNs)**
5. Click **Continue** and then **Register**
6. **Download the key file** (.p8 file) - you can only download it once!
7. Note down the **Key ID** (displayed on the download page)
8. Note down your **Team ID** (found in the top-right corner of the developer portal)

### Option B: APNs Certificate (Legacy)

1. In the Apple Developer Portal, go to **Certificates**
2. Click the **+** button to create a new certificate
3. Under **Services**, select **Apple Push Notification service SSL**
4. Choose **Sandbox** for development or **Production** for release
5. Select your App ID and click **Continue**
6. Create a Certificate Signing Request (CSR) using Keychain Access on macOS
7. Upload the CSR and download the certificate
8. Install the certificate in Keychain Access
9. Export as .p12 file with a password

## Step 3: Configure Your iOS App

### Enable Push Notifications Capability

1. Open your iOS project in Xcode
2. Select your project in the navigator
3. Select your app target
4. Go to **Signing & Capabilities**
5. Click **+ Capability** and add **Push Notifications**

### Request Permission and Get Device Token

Add this code to your iOS app:

```swift
import UserNotifications

class AppDelegate: UIResponder, UIApplicationDelegate {
    
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        
        // Request notification permission
        UNUserNotificationCenter.current().requestAuthorization(options: [.alert, .badge, .sound]) { granted, error in
            if granted {
                DispatchQueue.main.async {
                    application.registerForRemoteNotifications()
                }
            }
        }
        
        return true
    }
    
    // Called when device token is received
    func application(_ application: UIApplication, didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
        let tokenString = deviceToken.map { String(format: "%02.2hhx", $0) }.joined()
        print("Device Token: \(tokenString)")
        
        // Send this token to your server
        sendTokenToServer(tokenString)
    }
    
    // Called when registration fails
    func application(_ application: UIApplication, didFailToRegisterForRemoteNotificationsWithError error: Error) {
        print("Failed to register for remote notifications: \(error)")
    }
    
    private func sendTokenToServer(_ token: String) {
        // Send the device token to your server
        // Example API call to your push notification service
        let url = URL(string: "https://your-api.com/api/subscription/subscribe")!
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = [
            "userId": 123, // Your user ID
            "deviceToken": token,
            "platform": "ios"
        ]
        
        request.httpBody = try? JSONSerialization.data(withJSONObject: body)
        
        URLSession.shared.dataTask(with: request).resume()
    }
}
```

## Step 4: Configure the .NET Application

### Using APNs Authentication Key (Recommended)

1. Convert your .p8 key file to Base64:

```bash
# On macOS/Linux
base64 -i AuthKey_XXXXXXXXXX.p8 -o key.txt

# On Windows (PowerShell)
[Convert]::ToBase64String([IO.File]::ReadAllBytes("AuthKey_XXXXXXXXXX.p8")) | Out-File key.txt
```

2. Update your `appsettings.json`:

```json
{
  "PushNotification": {
    "Apple": {
      "TeamId": "YOUR_TEAM_ID",
      "KeyId": "YOUR_KEY_ID",
      "PrivateKey": "YOUR_BASE64_ENCODED_P8_KEY_CONTENT",
      "BundleId": "com.yourcompany.yourapp",
      "UseSandbox": true
    }
  }
}
```

### Using APNs Certificate (Legacy)

If using certificates instead of keys, you'll need to implement certificate-based authentication. The current library uses the key-based approach which is recommended by Apple.

## Step 5: Environment Configuration

### Development (Sandbox)
- Set `"UseSandbox": true` in configuration
- Use development certificates/keys
- Test with apps installed via Xcode or TestFlight (internal testing)

### Production
- Set `"UseSandbox": false` in configuration
- Use production certificates/keys
- Test with apps distributed via App Store or TestFlight (external testing)

## Step 6: Testing

### Test with the Web API

1. Start your .NET Web API application
2. Register a device token:

```bash
curl -X POST "https://localhost:5001/api/subscription/subscribe" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 123,
    "deviceToken": "your-ios-device-token-here",
    "platform": "ios"
  }'
```

3. Send a test notification:

```bash
curl -X POST "https://localhost:5001/api/notification/send-test?userIds=123" \
  -H "Content-Type: application/json"
```

### Verify Notification Delivery

1. Check your iOS device for the notification
2. Check the API logs for delivery status
3. Monitor APNs response codes in the logs

## Common Issues and Troubleshooting

### Invalid Device Token
- **Cause**: Device token has changed or is invalid
- **Solution**: Re-register the device token from the iOS app

### Invalid Bundle ID
- **Cause**: Bundle ID in configuration doesn't match the app
- **Solution**: Verify Bundle ID matches exactly (case-sensitive)

### Certificate/Key Issues
- **Cause**: Expired or invalid certificates/keys
- **Solution**: Regenerate certificates/keys in Apple Developer Portal

### Sandbox vs Production Mismatch
- **Cause**: Using sandbox configuration with production app or vice versa
- **Solution**: Ensure configuration matches app distribution method

### Network Issues
- **Cause**: Firewall blocking APNs endpoints
- **Solution**: Ensure outbound HTTPS access to:
  - `api.push.apple.com:443` (production)
  - `api.sandbox.push.apple.com:443` (sandbox)

## Security Best Practices

1. **Secure Key Storage**: Store private keys securely (Azure Key Vault, AWS Secrets Manager, etc.)
2. **Key Rotation**: Regularly rotate APNs keys
3. **Environment Separation**: Use different keys for development and production
4. **Access Control**: Limit access to APNs keys and certificates
5. **Monitoring**: Monitor for failed deliveries and invalid tokens

## APNs Response Codes

| Status Code | Meaning | Action |
|-------------|---------|---------|
| 200 | Success | Notification delivered |
| 400 | Bad Request | Check payload format |
| 403 | Forbidden | Check certificate/key |
| 410 | Gone | Remove invalid device token |
| 413 | Payload Too Large | Reduce payload size |
| 429 | Too Many Requests | Implement rate limiting |
| 500 | Internal Server Error | Retry later |
| 503 | Service Unavailable | Retry later |

## Additional Resources

- [Apple Push Notification Service Documentation](https://developer.apple.com/documentation/usernotifications)
- [APNs Provider API](https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server)
- [Generating a Universal Push Notification Client SSL Certificate](https://developer.apple.com/documentation/usernotifications/setting_up_a_remote_notification_server/establishing_a_certificate-based_connection_to_apns)
