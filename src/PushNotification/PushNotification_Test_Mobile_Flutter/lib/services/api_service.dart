import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:io';

class ApiService {
  static String _baseUrl = 'https://10.0.2.2:5001'; // Android emulator localhost
  static int _userId = 1;

  static String get baseUrl => _baseUrl;
  static int get userId => _userId;

  static void setBaseUrl(String url) {
    _baseUrl = url;
    _saveSettings();
  }

  static void setUserId(int id) {
    _userId = id;
    _saveSettings();
  }

  static Future<void> loadSettings() async {
    final prefs = await SharedPreferences.getInstance();
    _baseUrl = prefs.getString('api_base_url') ?? _baseUrl;
    _userId = prefs.getInt('user_id') ?? _userId;
  }

  static Future<void> _saveSettings() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('api_base_url', _baseUrl);
    await prefs.setInt('user_id', _userId);
  }

  static Future<Map<String, dynamic>> testConnection() async {
    try {
      final response = await http.get(
        Uri.parse('$_baseUrl/api/health'),
        headers: {'Content-Type': 'application/json'},
      ).timeout(const Duration(seconds: 10));

      if (response.statusCode == 200) {
        return {
          'success': true,
          'data': json.decode(response.body),
          'message': 'Connection successful'
        };
      } else {
        return {
          'success': false,
          'error': 'HTTP ${response.statusCode}: ${response.reasonPhrase}'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Connection failed: $e'
      };
    }
  }

  static Future<Map<String, dynamic>> subscribe(String deviceToken) async {
    try {
      final platform = Platform.isIOS ? 'ios' : 'android';
      
      final response = await http.post(
        Uri.parse('$_baseUrl/api/subscription/subscribe'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'userId': _userId,
          'deviceToken': deviceToken,
          'platform': platform,
        }),
      ).timeout(const Duration(seconds: 10));

      final responseData = json.decode(response.body);

      if (response.statusCode == 200 && responseData['success'] == true) {
        return {
          'success': true,
          'message': responseData['message'] ?? 'Subscription successful'
        };
      } else {
        return {
          'success': false,
          'error': responseData['error'] ?? 'Subscription failed'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Subscription failed: $e'
      };
    }
  }

  static Future<Map<String, dynamic>> unsubscribe(String deviceToken) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/subscription/unsubscribe'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'userId': _userId,
          'deviceToken': deviceToken,
        }),
      ).timeout(const Duration(seconds: 10));

      final responseData = json.decode(response.body);

      if (response.statusCode == 200 && responseData['success'] == true) {
        return {
          'success': true,
          'message': responseData['message'] ?? 'Unsubscription successful'
        };
      } else {
        return {
          'success': false,
          'error': responseData['error'] ?? 'Unsubscription failed'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Unsubscription failed: $e'
      };
    }
  }

  static Future<Map<String, dynamic>> sendTestNotification() async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/notification/send-test?userIds=$_userId'),
        headers: {'Content-Type': 'application/json'},
      ).timeout(const Duration(seconds: 10));

      final responseData = json.decode(response.body);

      if (response.statusCode == 200 && responseData['success'] == true) {
        return {
          'success': true,
          'message': responseData['message'] ?? 'Test notification sent'
        };
      } else {
        return {
          'success': false,
          'error': responseData['error'] ?? 'Failed to send test notification'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Failed to send test notification: $e'
      };
    }
  }

  static Future<Map<String, dynamic>> sendCustomNotification({
    required String title,
    required String body,
    Map<String, dynamic>? customData,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/notification/send'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'title': title,
          'body': body,
          'userIds': [_userId],
          'customData': customData ?? {
            'source': 'flutter-app',
            'timestamp': DateTime.now().toIso8601String(),
          },
        }),
      ).timeout(const Duration(seconds: 10));

      final responseData = json.decode(response.body);

      if (response.statusCode == 200 && responseData['success'] == true) {
        return {
          'success': true,
          'message': responseData['message'] ?? 'Custom notification sent'
        };
      } else {
        return {
          'success': false,
          'error': responseData['error'] ?? 'Failed to send custom notification'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Failed to send custom notification: $e'
      };
    }
  }

  static Future<Map<String, dynamic>> sendBroadcastNotification({
    required String title,
    required String body,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/notification/send'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'title': title,
          'body': body,
          'sendToAll': true,
          'customData': {
            'source': 'flutter-app-broadcast',
            'timestamp': DateTime.now().toIso8601String(),
          },
        }),
      ).timeout(const Duration(seconds: 10));

      final responseData = json.decode(response.body);

      if (response.statusCode == 200 && responseData['success'] == true) {
        return {
          'success': true,
          'message': responseData['message'] ?? 'Broadcast notification sent'
        };
      } else {
        return {
          'success': false,
          'error': responseData['error'] ?? 'Failed to send broadcast notification'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Failed to send broadcast notification: $e'
      };
    }
  }

  static Future<Map<String, dynamic>> getUserSubscriptions() async {
    try {
      final response = await http.get(
        Uri.parse('$_baseUrl/api/subscription/user/$_userId'),
        headers: {'Content-Type': 'application/json'},
      ).timeout(const Duration(seconds: 10));

      final responseData = json.decode(response.body);

      if (response.statusCode == 200 && responseData['success'] == true) {
        return {
          'success': true,
          'data': responseData['data'] ?? [],
        };
      } else {
        return {
          'success': false,
          'error': responseData['error'] ?? 'Failed to get subscriptions'
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Failed to get subscriptions: $e'
      };
    }
  }
}
