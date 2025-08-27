import 'package:flutter/material.dart';
import '../services/notification_service.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  List<Map<String, dynamic>> _notifications = [];

  @override
  void initState() {
    super.initState();
    _loadNotifications();
    
    // Listen for new notifications
    NotificationService.setOnNotificationReceived((notification) {
      setState(() {
        _notifications = NotificationService.notifications;
      });
    });
  }

  void _loadNotifications() {
    setState(() {
      _notifications = NotificationService.notifications;
    });
  }

  void _clearNotifications() {
    NotificationService.clearNotifications();
    setState(() {
      _notifications = [];
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        title: const Text('Received Notifications'),
        actions: [
          IconButton(
            icon: const Icon(Icons.clear_all),
            onPressed: _notifications.isNotEmpty ? _clearNotifications : null,
            tooltip: 'Clear all notifications',
          ),
        ],
      ),
      body: _notifications.isEmpty
          ? const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.notifications_none,
                    size: 64,
                    color: Colors.grey,
                  ),
                  SizedBox(height: 16),
                  Text(
                    'No notifications received yet',
                    style: TextStyle(
                      fontSize: 18,
                      color: Colors.grey,
                    ),
                  ),
                  SizedBox(height: 8),
                  Text(
                    'Notifications will appear here when received',
                    style: TextStyle(
                      fontSize: 14,
                      color: Colors.grey,
                    ),
                  ),
                ],
              ),
            )
          : ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: _notifications.length,
              itemBuilder: (context, index) {
                final notification = _notifications[index];
                return Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: ExpansionTile(
                    leading: Icon(
                      notification['isBackground'] == true
                          ? Icons.notifications
                          : Icons.notification_important,
                      color: notification['isBackground'] == true
                          ? Colors.blue
                          : Colors.orange,
                    ),
                    title: Text(
                      notification['title'] ?? 'No Title',
                      style: const TextStyle(fontWeight: FontWeight.bold),
                    ),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(notification['body'] ?? 'No Body'),
                        const SizedBox(height: 4),
                        Text(
                          _formatTimestamp(notification['timestamp']),
                          style: const TextStyle(
                            fontSize: 12,
                            color: Colors.grey,
                          ),
                        ),
                      ],
                    ),
                    children: [
                      if (notification['data'] != null &&
                          (notification['data'] as Map).isNotEmpty)
                        Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text(
                                'Custom Data:',
                                style: TextStyle(fontWeight: FontWeight.bold),
                              ),
                              const SizedBox(height: 8),
                              Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(12),
                                decoration: BoxDecoration(
                                  color: Colors.grey[100],
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  _formatData(notification['data']),
                                  style: const TextStyle(
                                    fontFamily: 'monospace',
                                    fontSize: 12,
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: Row(
                          children: [
                            Icon(
                              notification['isBackground'] == true
                                  ? Icons.visibility_off
                                  : Icons.visibility,
                              size: 16,
                              color: Colors.grey,
                            ),
                            const SizedBox(width: 4),
                            Text(
                              notification['isBackground'] == true
                                  ? 'Background notification'
                                  : 'Foreground notification',
                              style: const TextStyle(
                                fontSize: 12,
                                color: Colors.grey,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
      floatingActionButton: FloatingActionButton(
        onPressed: _loadNotifications,
        tooltip: 'Refresh',
        child: const Icon(Icons.refresh),
      ),
    );
  }

  String _formatTimestamp(String? timestamp) {
    if (timestamp == null) return 'Unknown time';
    
    try {
      final dateTime = DateTime.parse(timestamp);
      final now = DateTime.now();
      final difference = now.difference(dateTime);
      
      if (difference.inMinutes < 1) {
        return 'Just now';
      } else if (difference.inHours < 1) {
        return '${difference.inMinutes}m ago';
      } else if (difference.inDays < 1) {
        return '${difference.inHours}h ago';
      } else {
        return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour}:${dateTime.minute.toString().padLeft(2, '0')}';
      }
    } catch (e) {
      return timestamp;
    }
  }

  String _formatData(dynamic data) {
    if (data == null) return 'null';
    
    if (data is Map) {
      final buffer = StringBuffer();
      data.forEach((key, value) {
        buffer.writeln('$key: $value');
      });
      return buffer.toString().trim();
    }
    
    return data.toString();
  }
}
