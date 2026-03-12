# Push Notification Test Web Client

A simple HTML/JavaScript web client for testing push notifications with the DotNetBrightener Push Notification library. This client demonstrates web push notification functionality using Firebase Cloud Messaging (FCM).

## Features

- **Firebase Integration**: Connect to Firebase Cloud Messaging for web push notifications
- **API Testing**: Test connection to the Push Notification Web API
- **Subscription Management**: Subscribe and unsubscribe from push notifications
- **Send Notifications**: Send test and custom notifications
- **Real-time Display**: Display received notifications in real-time
- **Activity Logging**: Comprehensive logging of all activities
- **Persistent Configuration**: Save Firebase configuration locally

## Setup

### 1. Firebase Configuration

Before using the web client, you need to set up Firebase:

1. Create a Firebase project at [Firebase Console](https://console.firebase.google.com/)
2. Enable Cloud Messaging
3. Generate a VAPID key pair in Project Settings > Cloud Messaging
4. Get your Firebase configuration values

### 2. Update Service Worker

Edit `firebase-messaging-sw.js` and replace the placeholder Firebase configuration with your actual values:

```javascript
const firebaseConfig = {
  apiKey: "your-actual-api-key",
  authDomain: "your-project.firebaseapp.com",
  projectId: "your-actual-project-id",
  storageBucket: "your-project.appspot.com",
  messagingSenderId: "your-actual-sender-id",
  appId: "your-actual-app-id"
};
```

### 3. Serve the Files

The web client must be served over HTTPS (or localhost) for push notifications to work. You can use:

#### Option A: Simple HTTP Server (for testing)
```bash
# Python 3
python -m http.server 8000

# Python 2
python -m SimpleHTTPServer 8000

# Node.js (if you have http-server installed)
npx http-server -p 8000

# PHP
php -S localhost:8000
```

#### Option B: Live Server (VS Code Extension)
1. Install the "Live Server" extension in VS Code
2. Right-click on `index.html` and select "Open with Live Server"

#### Option C: Web Server (Production)
Deploy the files to any web server (Apache, Nginx, IIS, etc.)

## Usage

### 1. Configure Firebase

1. Open the web client in your browser
2. Fill in the Firebase Configuration section:
   - **Project ID**: Your Firebase project ID
   - **API Key**: Your Firebase web API key
   - **Sender ID**: Your Firebase messaging sender ID
   - **VAPID Key**: Your Firebase VAPID key
3. Click "Initialize Firebase"

### 2. Test API Connection

1. Ensure your Push Notification Web API is running
2. Enter the API URL (default: `https://localhost:5001`)
3. Click "Test Connection"

### 3. Request Notification Permission

1. Click "Request Permission" to ask for browser notification permission
2. Allow notifications when prompted

### 4. Subscribe to Notifications

1. Enter a User ID (or use the default)
2. Click "Subscribe" to register for push notifications
3. The device token will appear in the text area

### 5. Send Test Notifications

1. Use "Send Test" for a quick test notification
2. Use "Send Custom" to send a notification with custom title and body
3. Use "Send to All" to broadcast to all subscribed users

### 6. Receive Notifications

- Foreground notifications appear in the "Received Notifications" section
- Background notifications appear as browser notifications
- All activity is logged in the "Activity Log" section

## File Structure

```
PushNotification_Test_WebClient/
├── index.html                 # Main HTML page
├── js/
│   └── app.js                 # Main JavaScript application
├── firebase-messaging-sw.js   # Firebase service worker
└── README.md                  # This file
```

## Browser Compatibility

The web client requires a modern browser with support for:
- Service Workers
- Push API
- Notifications API
- ES6+ JavaScript features

Supported browsers:
- Chrome 50+
- Firefox 44+
- Safari 16+ (with limitations)
- Edge 17+

## Troubleshooting

### Common Issues

1. **"Firebase not initialized"**
   - Ensure all Firebase configuration fields are filled
   - Check that the configuration values are correct
   - Verify the service worker is registered

2. **"Permission denied"**
   - Click "Request Permission" and allow notifications
   - Check browser notification settings
   - Try refreshing the page

3. **"No FCM token"**
   - Ensure Firebase is initialized
   - Check that notification permission is granted
   - Verify VAPID key is correct

4. **"API connection failed"**
   - Ensure the Web API is running
   - Check the API URL is correct
   - Verify CORS is enabled on the API

5. **"Service worker registration failed"**
   - Ensure files are served over HTTPS or localhost
   - Check browser console for detailed errors
   - Verify service worker file path is correct

### Debug Tips

1. **Check Browser Console**: Open Developer Tools (F12) and check the Console tab for errors
2. **Check Network Tab**: Verify API requests are being made successfully
3. **Check Application Tab**: In Developer Tools, check Service Workers and Storage sections
4. **Test in Incognito**: Try the client in an incognito/private window to rule out cache issues

## Security Considerations

- Always serve over HTTPS in production
- Keep Firebase configuration secure
- Validate all user inputs
- Implement proper authentication for production use
- Monitor for abuse and implement rate limiting

## Development

### Adding New Features

1. **New API Endpoints**: Add functions in `app.js` to call new API endpoints
2. **UI Components**: Add HTML elements and corresponding JavaScript handlers
3. **Notification Types**: Extend the notification display logic for different types
4. **Error Handling**: Add specific error handling for different scenarios

### Testing

1. Test with different browsers
2. Test foreground and background notifications
3. Test with network offline/online scenarios
4. Test permission granted/denied scenarios
5. Test with different notification payloads

## Integration with Mobile Apps

This web client can be used alongside mobile applications:
- Use the same Firebase project for web and mobile
- Test cross-platform notification delivery
- Verify consistent behavior across platforms
- Test different notification formats and data payloads
