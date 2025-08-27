# Push Notification Test - Flutter Mobile App

A Flutter mobile application for testing push notifications with the DotNetBrightener Push Notification library. This app demonstrates both Firebase Cloud Messaging (FCM) and Apple Push Notification service (APNs) integration.

## Features

- **Cross-Platform**: Works on both iOS and Android
- **Firebase Integration**: FCM for Android and iOS push notifications
- **Real-time Notifications**: Handle foreground and background notifications
- **API Integration**: Connect to the Push Notification Web API
- **Subscription Management**: Subscribe/unsubscribe from push notifications
- **Notification History**: View all received notifications
- **Settings Management**: Configure API endpoints and user settings
- **Permission Handling**: Request and manage notification permissions
- **Local Notifications**: Display notifications when app is in foreground

## Prerequisites

- Flutter SDK (3.0.0 or higher)
- Dart SDK (3.0.0 or higher)
- Android Studio / Xcode for platform-specific development
- Firebase project with FCM enabled
- DotNetBrightener Push Notification Web API running

## Setup

### 1. Flutter Environment

Ensure Flutter is installed and configured:

```bash
flutter doctor
```

### 2. Firebase Configuration

#### Create Firebase Project
1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Create a new project or use existing one
3. Enable Cloud Messaging

#### Android Setup
1. Add Android app to Firebase project
2. Download `google-services.json`
3. Place it in `android/app/`
4. Update `android/app/build.gradle` with Firebase plugin

#### iOS Setup
1. Add iOS app to Firebase project
2. Download `GoogleService-Info.plist`
3. Add it to `ios/Runner/` in Xcode
4. Enable Push Notifications capability in Xcode

### 3. Update Firebase Configuration

Edit `lib/firebase_options.dart` with your actual Firebase configuration:

```dart
static const FirebaseOptions android = FirebaseOptions(
  apiKey: 'your-actual-android-api-key',
  appId: 'your-actual-android-app-id',
  messagingSenderId: 'your-actual-sender-id',
  projectId: 'your-actual-project-id',
  storageBucket: 'your-actual-project.appspot.com',
);

static const FirebaseOptions ios = FirebaseOptions(
  apiKey: 'your-actual-ios-api-key',
  appId: 'your-actual-ios-app-id',
  messagingSenderId: 'your-actual-sender-id',
  projectId: 'your-actual-project-id',
  storageBucket: 'your-actual-project.appspot.com',
  iosBundleId: 'com.yourcompany.pushnotificationtest',
);
```

### 4. Install Dependencies

```bash
flutter pub get
```

### 5. Platform-Specific Setup

#### Android
- Minimum SDK version: 21 (Android 5.0)
- Target SDK version: 34
- Compile SDK version: 34

#### iOS
- Minimum iOS version: 12.0
- Xcode 14.0 or later
- Valid Apple Developer account for push notifications

## Running the App

### Development

```bash
# Run on connected device/emulator
flutter run

# Run in debug mode
flutter run --debug

# Run in release mode
flutter run --release
```

### Building

```bash
# Build APK for Android
flutter build apk

# Build iOS (requires macOS and Xcode)
flutter build ios
```

## Usage

### 1. Initial Setup

1. Launch the app
2. Go to Settings (gear icon in top-right)
3. Configure API Base URL (default: `https://10.0.2.2:5001` for Android emulator)
4. Set User ID (default: 1)
5. Save settings

### 2. Test API Connection

1. In Settings, tap "Test Connection"
2. Verify connection is successful
3. If failed, check API URL and ensure Web API is running

### 3. Request Permissions

1. In Settings, tap "Request Notification Permissions"
2. Allow notifications when prompted
3. FCM token should appear automatically

### 4. Subscribe to Notifications

1. Return to Home screen
2. Tap "Subscribe" button
3. Verify subscription is successful

### 5. Send Test Notifications

1. Use "Send Test" for quick test
2. Enter custom title/body and use "Send Custom"
3. Check "Received Notifications" screen for incoming notifications

### 6. View Notifications

1. Tap notifications icon in app bar
2. View all received notifications
3. Expand notifications to see custom data
4. Clear notifications as needed

## Project Structure

```
lib/
├── main.dart                 # App entry point
├── firebase_options.dart     # Firebase configuration
├── screens/
│   ├── home_screen.dart      # Main app screen
│   ├── notifications_screen.dart # Notification history
│   └── settings_screen.dart  # App settings
└── services/
    ├── notification_service.dart # FCM and local notifications
    └── api_service.dart      # API communication
```

## Key Components

### NotificationService
- Handles FCM token management
- Processes foreground/background messages
- Manages local notifications
- Handles permission requests

### ApiService
- Communicates with Push Notification Web API
- Manages subscription/unsubscription
- Sends test notifications
- Handles API configuration

### Screens
- **HomeScreen**: Main interface for testing
- **NotificationsScreen**: History of received notifications
- **SettingsScreen**: Configuration and device info

## Testing Scenarios

### Foreground Notifications
1. Keep app open and in focus
2. Send notification from Web API or another device
3. Notification should appear in app and as local notification

### Background Notifications
1. Minimize app or switch to another app
2. Send notification
3. Should receive system notification
4. Tap notification to open app

### App Terminated
1. Force close the app
2. Send notification
3. Should receive system notification
4. Tap to launch app

## Troubleshooting

### Common Issues

1. **No FCM Token**
   - Check Firebase configuration
   - Ensure Google Services plugin is applied
   - Verify internet connection

2. **Permissions Denied**
   - Go to device Settings > Apps > [App Name] > Notifications
   - Enable notifications manually
   - Restart app

3. **API Connection Failed**
   - Check API URL in settings
   - Ensure Web API is running
   - For Android emulator, use `10.0.2.2` instead of `localhost`
   - For iOS simulator, use actual IP address

4. **Notifications Not Received**
   - Check subscription status
   - Verify FCM token is valid
   - Check device notification settings
   - Ensure app has notification permissions

### Debug Tips

1. **Check Logs**: Use `flutter logs` to see detailed logs
2. **Firebase Console**: Check FCM send status in Firebase Console
3. **Device Logs**: Use platform-specific tools (adb logcat, Xcode console)
4. **Network**: Verify API endpoints are reachable

## Platform Differences

### Android
- Uses FCM for all notifications
- Background notifications handled by system
- Custom notification channels supported
- Rich notification features available

### iOS
- Can use FCM or APNs (configured in Web API)
- Background notifications require APNs configuration
- Notification permissions more restrictive
- Rich notifications require additional setup

## Security Considerations

- Store Firebase configuration securely
- Use HTTPS for API communication
- Validate notification payloads
- Implement proper authentication for production
- Monitor for abuse and implement rate limiting

## Production Deployment

1. **Firebase**: Switch to production Firebase project
2. **API**: Update API URLs to production endpoints
3. **Certificates**: Use production APNs certificates
4. **Testing**: Test thoroughly on physical devices
5. **Store Submission**: Follow platform-specific guidelines

## Integration with Other Platforms

This Flutter app works alongside:
- Web client for browser push notifications
- .NET Web API for notification management
- Other mobile platforms using the same API

Test cross-platform notification delivery and ensure consistent behavior across all platforms.
