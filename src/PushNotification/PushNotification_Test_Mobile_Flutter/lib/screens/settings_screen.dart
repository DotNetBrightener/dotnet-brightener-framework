import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../services/api_service.dart';
import '../services/notification_service.dart';
import 'dart:io';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  final TextEditingController _apiUrlController = TextEditingController();
  final TextEditingController _userIdController = TextEditingController();
  
  bool _isLoading = false;
  String _platform = '';
  String _fcmToken = '';

  @override
  void initState() {
    super.initState();
    _loadSettings();
  }

  void _loadSettings() {
    _apiUrlController.text = ApiService.baseUrl;
    _userIdController.text = ApiService.userId.toString();
    _platform = Platform.isIOS ? 'iOS' : 'Android';
    _fcmToken = NotificationService.fcmToken ?? 'Not available';
  }

  Future<void> _saveSettings() async {
    final apiUrl = _apiUrlController.text.trim();
    final userIdText = _userIdController.text.trim();

    if (apiUrl.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('API URL cannot be empty')),
      );
      return;
    }

    final userId = int.tryParse(userIdText);
    if (userId == null || userId <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter a valid User ID')),
      );
      return;
    }

    ApiService.setBaseUrl(apiUrl);
    ApiService.setUserId(userId);

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Settings saved successfully')),
    );
  }

  Future<void> _testConnection() async {
    setState(() {
      _isLoading = true;
    });

    final result = await ApiService.testConnection();
    
    setState(() {
      _isLoading = false;
    });

    if (result['success']) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Connection successful!'),
          backgroundColor: Colors.green,
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Connection failed: ${result['error']}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  Future<void> _requestPermissions() async {
    setState(() {
      _isLoading = true;
    });

    await NotificationService.requestPermissions();
    
    setState(() {
      _isLoading = false;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Permission request completed')),
    );
  }

  void _copyToken() {
    if (_fcmToken.isNotEmpty && _fcmToken != 'Not available') {
      Clipboard.setData(ClipboardData(text: _fcmToken));
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Token copied to clipboard')),
      );
    }
  }

  Future<void> _getUserSubscriptions() async {
    setState(() {
      _isLoading = true;
    });

    final result = await ApiService.getUserSubscriptions();
    
    setState(() {
      _isLoading = false;
    });

    if (result['success']) {
      final subscriptions = result['data'] as List;
      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('User Subscriptions'),
          content: subscriptions.isEmpty
              ? const Text('No subscriptions found')
              : SizedBox(
                  width: double.maxFinite,
                  child: ListView.builder(
                    shrinkWrap: true,
                    itemCount: subscriptions.length,
                    itemBuilder: (context, index) {
                      final subscription = subscriptions[index];
                      return ListTile(
                        leading: Icon(
                          subscription['platform'] == 'ios'
                              ? Icons.phone_iphone
                              : Icons.phone_android,
                        ),
                        title: Text('Platform: ${subscription['platform']}'),
                        subtitle: Text(
                          'Token: ${subscription['deviceToken'].substring(0, 20)}...',
                        ),
                      );
                    },
                  ),
                ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Close'),
            ),
          ],
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Failed to get subscriptions: ${result['error']}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        title: const Text('Settings'),
        actions: [
          IconButton(
            icon: const Icon(Icons.save),
            onPressed: _saveSettings,
            tooltip: 'Save Settings',
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // API Configuration Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'API Configuration',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _apiUrlController,
                      decoration: const InputDecoration(
                        labelText: 'API Base URL',
                        hintText: 'https://your-api.com',
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.link),
                      ),
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _userIdController,
                      decoration: const InputDecoration(
                        labelText: 'User ID',
                        hintText: '1',
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.person),
                      ),
                      keyboardType: TextInputType.number,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      onPressed: _isLoading ? null : _testConnection,
                      icon: _isLoading
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.wifi),
                      label: const Text('Test Connection'),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // Device Information Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Device Information',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    ListTile(
                      leading: Icon(
                        Platform.isIOS ? Icons.phone_iphone : Icons.phone_android,
                      ),
                      title: const Text('Platform'),
                      subtitle: Text(_platform),
                    ),
                    ListTile(
                      leading: const Icon(Icons.token),
                      title: const Text('FCM Token'),
                      subtitle: Text(
                        _fcmToken.length > 50
                            ? '${_fcmToken.substring(0, 50)}...'
                            : _fcmToken,
                      ),
                      trailing: IconButton(
                        icon: const Icon(Icons.copy),
                        onPressed: _copyToken,
                      ),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // Permissions Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Permissions',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      onPressed: _isLoading ? null : _requestPermissions,
                      icon: const Icon(Icons.security),
                      label: const Text('Request Notification Permissions'),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // Subscription Management Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Subscription Management',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      onPressed: _isLoading ? null : _getUserSubscriptions,
                      icon: const Icon(Icons.list),
                      label: const Text('View User Subscriptions'),
                    ),
                  ],
                ),
              ),
            ),
            
            const SizedBox(height: 16),
            
            // App Information Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'App Information',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 16),
                    const ListTile(
                      leading: Icon(Icons.info),
                      title: Text('Version'),
                      subtitle: Text('1.0.0+1'),
                    ),
                    const ListTile(
                      leading: Icon(Icons.code),
                      title: Text('Framework'),
                      subtitle: Text('Flutter'),
                    ),
                    ListTile(
                      leading: const Icon(Icons.build),
                      title: const Text('Build Mode'),
                      subtitle: Text(
                        const bool.fromEnvironment('dart.vm.product')
                            ? 'Release'
                            : 'Debug',
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
    _apiUrlController.dispose();
    _userIdController.dispose();
    super.dispose();
  }
}
