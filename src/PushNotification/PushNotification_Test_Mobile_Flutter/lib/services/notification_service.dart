import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:io';

class NotificationService {
  static final FirebaseMessaging _firebaseMessaging = FirebaseMessaging.instance;
  static final FlutterLocalNotificationsPlugin _localNotifications = 
      FlutterLocalNotificationsPlugin();
  
  static String? _fcmToken;
  static List<Map<String, dynamic>> _notifications = [];
  static Function(Map<String, dynamic>)? _onNotificationReceived;
  static Function(String)? _onTokenReceived;

  static String? get fcmToken => _fcmToken;
  static List<Map<String, dynamic>> get notifications => _notifications;

  static void setOnNotificationReceived(Function(Map<String, dynamic>) callback) {
    _onNotificationReceived = callback;
  }

  static void setOnTokenReceived(Function(String) callback) {
    _onTokenReceived = callback;
  }

  static Future<void> initialize() async {
    // Initialize local notifications
    const AndroidInitializationSettings initializationSettingsAndroid =
        AndroidInitializationSettings('@mipmap/ic_launcher');
    
    const DarwinInitializationSettings initializationSettingsIOS =
        DarwinInitializationSettings(
      requestAlertPermission: true,
      requestBadgePermission: true,
      requestSoundPermission: true,
    );

    const InitializationSettings initializationSettings =
        InitializationSettings(
      android: initializationSettingsAndroid,
      iOS: initializationSettingsIOS,
    );

    await _localNotifications.initialize(
      initializationSettings,
      onDidReceiveNotificationResponse: _onNotificationTapped,
    );

    // Request permissions
    await requestPermissions();

    // Get FCM token
    await _getFCMToken();

    // Listen for token refresh
    _firebaseMessaging.onTokenRefresh.listen((token) {
      _fcmToken = token;
      _saveTokenToPrefs(token);
      _onTokenReceived?.call(token);
      print('FCM Token refreshed: $token');
    });

    // Handle foreground messages
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      print('Received foreground message: ${message.messageId}');
      _handleMessage(message, true);
    });

    // Handle notification taps when app is in background
    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      print('Notification tapped: ${message.messageId}');
      _handleMessage(message, false);
    });

    // Check for initial message (when app is opened from terminated state)
    RemoteMessage? initialMessage = await _firebaseMessaging.getInitialMessage();
    if (initialMessage != null) {
      print('App opened from notification: ${initialMessage.messageId}');
      _handleMessage(initialMessage, false);
    }
  }

  static Future<void> requestPermissions() async {
    // Request notification permissions
    NotificationSettings settings = await _firebaseMessaging.requestPermission(
      alert: true,
      announcement: false,
      badge: true,
      carPlay: false,
      criticalAlert: false,
      provisional: false,
      sound: true,
    );

    print('Notification permission status: ${settings.authorizationStatus}');

    // Request additional permissions for Android
    if (Platform.isAndroid) {
      await Permission.notification.request();
    }
  }

  static Future<void> _getFCMToken() async {
    try {
      String? token = await _firebaseMessaging.getToken();
      if (token != null) {
        _fcmToken = token;
        await _saveTokenToPrefs(token);
        _onTokenReceived?.call(token);
        print('FCM Token: $token');
      }
    } catch (e) {
      print('Error getting FCM token: $e');
    }
  }

  static Future<void> _saveTokenToPrefs(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('fcm_token', token);
  }

  static Future<String?> getTokenFromPrefs() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('fcm_token');
  }

  static void _handleMessage(RemoteMessage message, bool showLocalNotification) {
    final notification = {
      'id': message.messageId ?? DateTime.now().millisecondsSinceEpoch.toString(),
      'title': message.notification?.title ?? 'No Title',
      'body': message.notification?.body ?? 'No Body',
      'data': message.data,
      'timestamp': DateTime.now().toIso8601String(),
      'isBackground': !showLocalNotification,
    };

    _notifications.insert(0, notification);
    _onNotificationReceived?.call(notification);

    if (showLocalNotification) {
      showNotification(
        title: notification['title'],
        body: notification['body'],
        payload: notification['data'].toString(),
      );
    }
  }

  static Future<void> showNotification({
    required String title,
    required String body,
    String? payload,
  }) async {
    const AndroidNotificationDetails androidPlatformChannelSpecifics =
        AndroidNotificationDetails(
      'push_notification_test',
      'Push Notification Test',
      channelDescription: 'Channel for push notification testing',
      importance: Importance.max,
      priority: Priority.high,
      showWhen: true,
    );

    const DarwinNotificationDetails iOSPlatformChannelSpecifics =
        DarwinNotificationDetails(
      presentAlert: true,
      presentBadge: true,
      presentSound: true,
    );

    const NotificationDetails platformChannelSpecifics = NotificationDetails(
      android: androidPlatformChannelSpecifics,
      iOS: iOSPlatformChannelSpecifics,
    );

    await _localNotifications.show(
      DateTime.now().millisecondsSinceEpoch ~/ 1000,
      title,
      body,
      platformChannelSpecifics,
      payload: payload,
    );
  }

  static void _onNotificationTapped(NotificationResponse response) {
    print('Notification tapped with payload: ${response.payload}');
    // Handle notification tap
  }

  static void clearNotifications() {
    _notifications.clear();
  }

  static Future<void> subscribeToTopic(String topic) async {
    await _firebaseMessaging.subscribeToTopic(topic);
    print('Subscribed to topic: $topic');
  }

  static Future<void> unsubscribeFromTopic(String topic) async {
    await _firebaseMessaging.unsubscribeFromTopic(topic);
    print('Unsubscribed from topic: $topic');
  }
}
