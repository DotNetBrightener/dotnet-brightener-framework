import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../services/notification_service.dart';
import '../services/api_service.dart';
import 'notifications_screen.dart';
import 'settings_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String? _fcmToken;
  bool _isSubscribed = false;
  bool _isLoading = false;
  String _connectionStatus = 'Disconnected';
  Color _connectionColor = Colors.red;
  List<String> _activityLog = [];

  final TextEditingController _titleController = TextEditingController();
  final TextEditingController _bodyController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _initializeApp();
  }

  Future<void> _initializeApp() async {
    await ApiService.loadSettings();
    
    // Set up notification callbacks
    NotificationService.setOnTokenReceived((token) {
      setState(() {
        _fcmToken = token;
      });
      _addToLog('FCM Token received');
    });

    NotificationService.setOnNotificationReceived((notification) {
      _addToLog('Notification received: ${notification['title']}');
    });

    // Get existing token
    final token = NotificationService.fcmToken;
    if (token != null) {
      setState(() {
        _fcmToken = token;
      });
    }

    _addToLog('App initialized');
  }

  void _addToLog(String message) {
    setState(() {
      _activityLog.insert(0, '${DateTime.now().toString().substring(11, 19)} - $message');
      if (_activityLog.length > 50) {
        _activityLog.removeLast();
      }
    });
  }

  Future<void> _testConnection() async {
    setState(() {
      _isLoading = true;
      _connectionStatus = 'Testing...';
      _connectionColor = Colors.orange;
    });

    final result = await ApiService.testConnection();
    
    setState(() {
      _isLoading = false;
      if (result['success']) {
        _connectionStatus = 'Connected';
        _connectionColor = Colors.green;
        _addToLog('API connection successful');
      } else {
        _connectionStatus = 'Failed';
        _connectionColor = Colors.red;
        _addToLog('API connection failed: ${result['error']}');
      }
    });
  }

  Future<void> _subscribe() async {
    if (_fcmToken == null) {
      _addToLog('No FCM token available');
      return;
    }

    setState(() {
      _isLoading = true;
    });

    final result = await ApiService.subscribe(_fcmToken!);
    
    setState(() {
      _isLoading = false;
      if (result['success']) {
        _isSubscribed = true;
        _addToLog('Successfully subscribed');
      } else {
        _addToLog('Subscription failed: ${result['error']}');
      }
    });

    if (result['success']) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Successfully subscribed to notifications')),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Subscription failed: ${result['error']}')),
      );
    }
  }

  Future<void> _unsubscribe() async {
    if (_fcmToken == null) {
      _addToLog('No FCM token available');
      return;
    }

    setState(() {
      _isLoading = true;
    });

    final result = await ApiService.unsubscribe(_fcmToken!);
    
    setState(() {
      _isLoading = false;
      if (result['success']) {
        _isSubscribed = false;
        _addToLog('Successfully unsubscribed');
      } else {
        _addToLog('Unsubscription failed: ${result['error']}');
      }
    });

    if (result['success']) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Successfully unsubscribed from notifications')),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Unsubscription failed: ${result['error']}')),
      );
    }
  }

  Future<void> _sendTestNotification() async {
    setState(() {
      _isLoading = true;
    });

    final result = await ApiService.sendTestNotification();
    
    setState(() {
      _isLoading = false;
    });

    if (result['success']) {
      _addToLog('Test notification sent');
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Test notification sent')),
      );
    } else {
      _addToLog('Failed to send test notification: ${result['error']}');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to send: ${result['error']}')),
      );
    }
  }

  Future<void> _sendCustomNotification() async {
    if (_titleController.text.isEmpty || _bodyController.text.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter both title and body')),
      );
      return;
    }

    setState(() {
      _isLoading = true;
    });

    final result = await ApiService.sendCustomNotification(
      title: _titleController.text,
      body: _bodyController.text,
    );
    
    setState(() {
      _isLoading = false;
    });

    if (result['success']) {
      _addToLog('Custom notification sent');
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Custom notification sent')),
      );
    } else {
      _addToLog('Failed to send custom notification: ${result['error']}');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to send: ${result['error']}')),
      );
    }
  }

  void _copyTokenToClipboard() {
    if (_fcmToken != null) {
      Clipboard.setData(ClipboardData(text: _fcmToken!));
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Token copied to clipboard')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        title: const Text('Push Notification Test'),
        actions: [
          IconButton(
            icon: const Icon(Icons.notifications),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const NotificationsScreen()),
              );
            },
          ),
          IconButton(
            icon: const Icon(Icons.settings),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const SettingsScreen()),
              );
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Connection Status Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Icon(Icons.wifi, color: _connectionColor),
                        const SizedBox(width: 8),
                        Text(
                          'Connection Status: $_connectionStatus',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: _isLoading ? null : _testConnection,
                      child: _isLoading
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Test Connection'),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // FCM Token Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'FCM Token',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: Colors.grey[100],
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        _fcmToken ?? 'No token available',
                        style: const TextStyle(
                          fontFamily: 'monospace',
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const SizedBox(height: 8),
                    ElevatedButton.icon(
                      onPressed: _fcmToken != null ? _copyTokenToClipboard : null,
                      icon: const Icon(Icons.copy),
                      label: const Text('Copy Token'),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // Subscription Management Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Subscription Management',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    Row(
                      children: [
                        Expanded(
                          child: ElevatedButton.icon(
                            onPressed: _isLoading || _fcmToken == null ? null : _subscribe,
                            icon: const Icon(Icons.notifications_active),
                            label: const Text('Subscribe'),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.green,
                              foregroundColor: Colors.white,
                            ),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: ElevatedButton.icon(
                            onPressed: _isLoading || _fcmToken == null ? null : _unsubscribe,
                            icon: const Icon(Icons.notifications_off),
                            label: const Text('Unsubscribe'),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.red,
                              foregroundColor: Colors.white,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // Send Notifications Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Send Test Notifications',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _titleController,
                      decoration: const InputDecoration(
                        labelText: 'Notification Title',
                        border: OutlineInputBorder(),
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: _bodyController,
                      decoration: const InputDecoration(
                        labelText: 'Notification Body',
                        border: OutlineInputBorder(),
                      ),
                      maxLines: 2,
                    ),
                    const SizedBox(height: 16),
                    Row(
                      children: [
                        Expanded(
                          child: ElevatedButton(
                            onPressed: _isLoading ? null : _sendTestNotification,
                            child: const Text('Send Test'),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: ElevatedButton(
                            onPressed: _isLoading ? null : _sendCustomNotification,
                            child: const Text('Send Custom'),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // Activity Log Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          'Activity Log',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        TextButton(
                          onPressed: () {
                            setState(() {
                              _activityLog.clear();
                            });
                          },
                          child: const Text('Clear'),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Container(
                      height: 200,
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: Colors.grey[100],
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: ListView.builder(
                        itemCount: _activityLog.length,
                        itemBuilder: (context, index) {
                          return Text(
                            _activityLog[index],
                            style: const TextStyle(
                              fontFamily: 'monospace',
                              fontSize: 12,
                            ),
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _titleController.dispose();
    _bodyController.dispose();
    super.dispose();
  }
}
