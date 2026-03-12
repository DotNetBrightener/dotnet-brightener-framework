# Firebase Cloud Messaging (FCM) Setup Guide

This guide walks you through setting up Firebase Cloud Messaging (FCM) for use with the DotNetBrightener Push Notification library. FCM supports Android, iOS, and Web platforms.

## Prerequisites

- Google account
- Android Studio (for Android development)
- Xcode (for iOS development)
- Web browser (for web push notifications)

## Step 1: Create a Firebase Project

1. Go to the [Firebase Console](https://console.firebase.google.com/)
2. Click **Create a project** or **Add project**
3. Enter your project name
4. Choose whether to enable Google Analytics (optional)
5. Select or create a Google Analytics account if enabled
6. Click **Create project**

## Step 2: Enable Firebase Cloud Messaging

1. In your Firebase project console, click on **Project settings** (gear icon)
2. Go to the **Cloud Messaging** tab
3. Note down the **Server key** and **Sender ID** (you'll need these later)

## Step 3: Create a Service Account

1. In the Firebase Console, go to **Project settings**
2. Click on the **Service accounts** tab
3. Click **Generate new private key**
4. Click **Generate key** to download the JSON file
5. Store this JSON file securely - it contains your service account credentials

## Step 4: Configure Your Applications

### Android Application

#### Add Firebase to Your Android Project

1. In the Firebase Console, click **Add app** and select Android
2. Enter your Android package name (e.g., `com.yourcompany.yourapp`)
3. Enter app nickname (optional)
4. Enter SHA-1 signing certificate fingerprint (optional, but recommended)
5. Click **Register app**
6. Download the `google-services.json` file
7. Place it in your Android project's `app/` directory

#### Add Firebase SDK

Add to your `app/build.gradle`:

```gradle
dependencies {
    implementation 'com.google.firebase:firebase-messaging:23.4.0'
    implementation 'com.google.firebase:firebase-analytics:21.5.0'
}
```

Add to your project-level `build.gradle`:

```gradle
buildscript {
    dependencies {
        classpath 'com.google.gms:google-services:4.4.0'
    }
}
```

Apply the plugin in your `app/build.gradle`:

```gradle
apply plugin: 'com.google.gms.google-services'
```

#### Implement FCM in Android

Create a service to handle FCM messages:

```kotlin
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import android.util.Log

class MyFirebaseMessagingService : FirebaseMessagingService() {

    override fun onMessageReceived(remoteMessage: RemoteMessage) {
        super.onMessageReceived(remoteMessage)
        
        Log.d(TAG, "From: ${remoteMessage.from}")
        
        // Check if message contains a notification payload
        remoteMessage.notification?.let {
            Log.d(TAG, "Message Notification Body: ${it.body}")
            showNotification(it.title, it.body)
        }
        
        // Check if message contains a data payload
        if (remoteMessage.data.isNotEmpty()) {
            Log.d(TAG, "Message data payload: ${remoteMessage.data}")
            handleDataMessage(remoteMessage.data)
        }
    }

    override fun onNewToken(token: String) {
        Log.d(TAG, "Refreshed token: $token")
        sendTokenToServer(token)
    }

    private fun sendTokenToServer(token: String) {
        // Send the token to your server
        // Example API call
        val retrofit = Retrofit.Builder()
            .baseUrl("https://your-api.com/")
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            
        val api = retrofit.create(PushNotificationApi::class.java)
        
        val request = SubscribeRequest(
            userId = getCurrentUserId(),
            deviceToken = token,
            platform = "android"
        )
        
        api.subscribe(request).enqueue(object : Callback<ApiResponse> {
            override fun onResponse(call: Call<ApiResponse>, response: Response<ApiResponse>) {
                Log.d(TAG, "Token sent to server successfully")
            }
            
            override fun onFailure(call: Call<ApiResponse>, t: Throwable) {
                Log.e(TAG, "Failed to send token to server", t)
            }
        })
    }

    companion object {
        private const val TAG = "MyFirebaseMsgService"
    }
}
```

Add the service to your `AndroidManifest.xml`:

```xml
<service
    android:name=".MyFirebaseMessagingService"
    android:exported="false">
    <intent-filter>
        <action android:name="com.google.firebase.MESSAGING_EVENT" />
    </intent-filter>
</service>
```

Get the FCM token in your main activity:

```kotlin
FirebaseMessaging.getInstance().token.addOnCompleteListener(OnCompleteListener { task ->
    if (!task.isSuccessful) {
        Log.w(TAG, "Fetching FCM registration token failed", task.exception)
        return@OnCompleteListener
    }

    // Get new FCM registration token
    val token = task.result
    Log.d(TAG, "FCM Registration Token: $token")
    
    // Send token to your server
    sendTokenToServer(token)
})
```

### iOS Application with FCM

#### Add Firebase to Your iOS Project

1. In the Firebase Console, click **Add app** and select iOS
2. Enter your iOS bundle ID
3. Enter app nickname (optional)
4. Download the `GoogleService-Info.plist` file
5. Add it to your Xcode project root

#### Add Firebase SDK

Add to your `Podfile`:

```ruby
pod 'Firebase/Messaging'
pod 'Firebase/Analytics'
```

Run `pod install`

#### Implement FCM in iOS

Configure Firebase in your `AppDelegate.swift`:

```swift
import Firebase
import FirebaseMessaging
import UserNotifications

@main
class AppDelegate: UIResponder, UIApplicationDelegate {

    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        
        FirebaseApp.configure()
        
        // Set messaging delegate
        Messaging.messaging().delegate = self
        
        // Request notification permissions
        UNUserNotificationCenter.current().delegate = self
        let authOptions: UNAuthorizationOptions = [.alert, .badge, .sound]
        UNUserNotificationCenter.current().requestAuthorization(options: authOptions) { granted, _ in
            if granted {
                DispatchQueue.main.async {
                    application.registerForRemoteNotifications()
                }
            }
        }
        
        return true
    }
    
    func application(_ application: UIApplication, didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
        Messaging.messaging().apnsToken = deviceToken
    }
}

extension AppDelegate: MessagingDelegate {
    func messaging(_ messaging: Messaging, didReceiveRegistrationToken fcmToken: String?) {
        guard let fcmToken = fcmToken else { return }
        print("Firebase registration token: \(fcmToken)")
        
        // Send token to your server
        sendTokenToServer(fcmToken)
    }
    
    private func sendTokenToServer(_ token: String) {
        // Send the FCM token to your server
        let url = URL(string: "https://your-api.com/api/subscription/subscribe")!
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = [
            "userId": getCurrentUserId(),
            "deviceToken": token,
            "platform": "ios"
        ]
        
        request.httpBody = try? JSONSerialization.data(withJSONObject: body)
        URLSession.shared.dataTask(with: request).resume()
    }
}

extension AppDelegate: UNUserNotificationCenterDelegate {
    func userNotificationCenter(_ center: UNUserNotificationCenter, willPresent notification: UNNotification, withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        completionHandler([[.alert, .sound]])
    }
    
    func userNotificationCenter(_ center: UNUserNotificationCenter, didReceive response: UNNotificationResponse, withCompletionHandler completionHandler: @escaping () -> Void) {
        let userInfo = response.notification.request.content.userInfo
        // Handle notification tap
        completionHandler()
    }
}
```

### Web Application

#### Add Firebase to Your Web Project

1. In the Firebase Console, click **Add app** and select Web
2. Enter your app nickname
3. Copy the Firebase configuration object

#### Implement FCM in Web

Create a `firebase-messaging-sw.js` file in your web root:

```javascript
importScripts('https://www.gstatic.com/firebasejs/9.0.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.0.0/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: "your-api-key",
  authDomain: "your-project.firebaseapp.com",
  projectId: "your-project-id",
  storageBucket: "your-project.appspot.com",
  messagingSenderId: "your-sender-id",
  appId: "your-app-id"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage(function(payload) {
  console.log('Received background message ', payload);
  
  const notificationTitle = payload.notification.title;
  const notificationOptions = {
    body: payload.notification.body,
    icon: '/firebase-logo.png'
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});
```

In your main JavaScript file:

```javascript
import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage } from 'firebase/messaging';

const firebaseConfig = {
  apiKey: "your-api-key",
  authDomain: "your-project.firebaseapp.com",
  projectId: "your-project-id",
  storageBucket: "your-project.appspot.com",
  messagingSenderId: "your-sender-id",
  appId: "your-app-id"
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

// Get registration token
getToken(messaging, { vapidKey: 'your-vapid-key' }).then((currentToken) => {
  if (currentToken) {
    console.log('Registration token:', currentToken);
    sendTokenToServer(currentToken);
  } else {
    console.log('No registration token available.');
  }
}).catch((err) => {
  console.log('An error occurred while retrieving token. ', err);
});

// Handle incoming messages
onMessage(messaging, (payload) => {
  console.log('Message received. ', payload);
  // Customize notification here
});

function sendTokenToServer(token) {
  fetch('https://your-api.com/api/subscription/subscribe', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      userId: getCurrentUserId(),
      deviceToken: token,
      platform: 'web'
    })
  });
}
```

## Step 5: Configure the .NET Application

Update your `appsettings.json`:

```json
{
  "PushNotification": {
    "Firebase": {
      "ProjectId": "your-firebase-project-id",
      "ServiceKey": "{ \"type\": \"service_account\", \"project_id\": \"your-project-id\", ... }",
      "SenderId": "your-sender-id",
      "EnableForIos": true
    }
  }
}
```

The `ServiceKey` should contain the entire JSON content from your service account file as a string.

## Step 6: Testing

### Test with the Web API

1. Register device tokens from your applications
2. Send test notifications using the API endpoints
3. Verify notifications are received on all platforms

### Test Commands

```bash
# Subscribe an Android device
curl -X POST "https://localhost:5001/api/subscription/subscribe" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 123,
    "deviceToken": "your-fcm-token-here",
    "platform": "android"
  }'

# Send test notification
curl -X POST "https://localhost:5001/api/notification/send-test?userIds=123"
```

## Troubleshooting

### Common Issues

1. **Invalid Registration Token**: Token has expired or is invalid
2. **Authentication Error**: Service account JSON is incorrect
3. **Project ID Mismatch**: Ensure project ID matches in all configurations
4. **Network Issues**: Check firewall settings for FCM endpoints

### FCM Response Codes

- **200**: Success
- **400**: Invalid JSON or missing required fields
- **401**: Authentication error
- **404**: Project not found
- **429**: Rate limit exceeded
- **500**: Internal server error

## Security Best Practices

1. **Secure Service Account**: Store service account JSON securely
2. **Token Management**: Regularly refresh and validate tokens
3. **Access Control**: Limit access to Firebase project
4. **Monitoring**: Monitor delivery rates and errors
5. **Data Privacy**: Be mindful of data sent in notifications

## Additional Resources

- [Firebase Cloud Messaging Documentation](https://firebase.google.com/docs/cloud-messaging)
- [FCM HTTP v1 API](https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages)
- [Firebase Console](https://console.firebase.google.com/)
