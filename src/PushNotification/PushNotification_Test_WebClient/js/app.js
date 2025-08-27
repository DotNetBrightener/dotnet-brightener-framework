// Global variables
let messaging = null;
let currentToken = null;
let isFirebaseInitialized = false;

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    logActivity('Application loaded', 'info');
    loadSavedConfiguration();
});

// Load saved configuration from localStorage
function loadSavedConfiguration() {
    const savedConfig = localStorage.getItem('firebaseConfig');
    if (savedConfig) {
        const config = JSON.parse(savedConfig);
        document.getElementById('projectId').value = config.projectId || '';
        document.getElementById('apiKey').value = config.apiKey || '';
        document.getElementById('messagingSenderId').value = config.messagingSenderId || '';
        document.getElementById('vapidKey').value = config.vapidKey || '';
        logActivity('Loaded saved Firebase configuration', 'info');
    }

    const savedApiUrl = localStorage.getItem('apiUrl');
    if (savedApiUrl) {
        document.getElementById('apiUrl').value = savedApiUrl;
    }

    const savedUserId = localStorage.getItem('userId');
    if (savedUserId) {
        document.getElementById('userId').value = savedUserId;
    }
}

// Save configuration to localStorage
function saveConfiguration() {
    const config = {
        projectId: document.getElementById('projectId').value,
        apiKey: document.getElementById('apiKey').value,
        messagingSenderId: document.getElementById('messagingSenderId').value,
        vapidKey: document.getElementById('vapidKey').value
    };
    localStorage.setItem('firebaseConfig', JSON.stringify(config));
    localStorage.setItem('apiUrl', document.getElementById('apiUrl').value);
    localStorage.setItem('userId', document.getElementById('userId').value);
}

// Test API connection
async function testConnection() {
    const apiUrl = document.getElementById('apiUrl').value;
    
    try {
        logActivity('Testing API connection...', 'info');
        updateConnectionStatus('pending', 'Testing...');
        
        const response = await fetch(`${apiUrl}/api/health`);
        
        if (response.ok) {
            const data = await response.json();
            updateConnectionStatus('connected', 'Connected');
            logActivity('API connection successful', 'success');
            logActivity(`API Status: ${data.data?.Status || 'Unknown'}`, 'info');
        } else {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
    } catch (error) {
        updateConnectionStatus('disconnected', 'Connection Failed');
        logActivity(`API connection failed: ${error.message}`, 'error');
    }
}

// Update connection status indicator
function updateConnectionStatus(status, text) {
    const indicator = document.getElementById('connectionStatus');
    const textElement = document.getElementById('connectionText');
    
    indicator.className = `status-indicator status-${status}`;
    textElement.textContent = text;
}

// Initialize Firebase
function initializeFirebase() {
    const projectId = document.getElementById('projectId').value;
    const apiKey = document.getElementById('apiKey').value;
    const messagingSenderId = document.getElementById('messagingSenderId').value;
    const vapidKey = document.getElementById('vapidKey').value;

    if (!projectId || !apiKey || !messagingSenderId || !vapidKey) {
        logActivity('Please fill in all Firebase configuration fields', 'error');
        return;
    }

    try {
        const firebaseConfig = {
            apiKey: apiKey,
            authDomain: `${projectId}.firebaseapp.com`,
            projectId: projectId,
            storageBucket: `${projectId}.appspot.com`,
            messagingSenderId: messagingSenderId,
            appId: `1:${messagingSenderId}:web:${Date.now()}`
        };

        // Initialize Firebase
        firebase.initializeApp(firebaseConfig);
        messaging = firebase.messaging();

        // Set up message handling
        messaging.onMessage((payload) => {
            logActivity('Received foreground message', 'success');
            displayNotification(payload);
        });

        // Get registration token
        messaging.getToken({ vapidKey: vapidKey }).then((token) => {
            if (token) {
                currentToken = token;
                document.getElementById('deviceToken').value = token;
                logActivity('Firebase initialized successfully', 'success');
                logActivity('FCM token received', 'success');
                isFirebaseInitialized = true;
                saveConfiguration();
            } else {
                logActivity('No registration token available', 'warning');
            }
        }).catch((err) => {
            logActivity(`Failed to get FCM token: ${err.message}`, 'error');
        });

    } catch (error) {
        logActivity(`Firebase initialization failed: ${error.message}`, 'error');
    }
}

// Request notification permission
async function requestPermission() {
    try {
        const permission = await Notification.requestPermission();
        
        if (permission === 'granted') {
            logActivity('Notification permission granted', 'success');
            if (isFirebaseInitialized) {
                // Refresh token after permission granted
                const vapidKey = document.getElementById('vapidKey').value;
                const token = await messaging.getToken({ vapidKey: vapidKey });
                if (token) {
                    currentToken = token;
                    document.getElementById('deviceToken').value = token;
                    logActivity('FCM token refreshed', 'success');
                }
            }
        } else {
            logActivity('Notification permission denied', 'warning');
        }
    } catch (error) {
        logActivity(`Error requesting permission: ${error.message}`, 'error');
    }
}

// Subscribe to notifications
async function subscribeToNotifications() {
    if (!currentToken) {
        logActivity('No FCM token available. Initialize Firebase first.', 'error');
        return;
    }

    const apiUrl = document.getElementById('apiUrl').value;
    const userId = parseInt(document.getElementById('userId').value);

    try {
        logActivity('Subscribing to notifications...', 'info');
        
        const response = await fetch(`${apiUrl}/api/subscription/subscribe`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userId: userId,
                deviceToken: currentToken,
                platform: 'web'
            })
        });

        const data = await response.json();
        
        if (response.ok && data.success) {
            logActivity('Successfully subscribed to notifications', 'success');
        } else {
            throw new Error(data.error || 'Subscription failed');
        }
    } catch (error) {
        logActivity(`Subscription failed: ${error.message}`, 'error');
    }
}

// Unsubscribe from notifications
async function unsubscribeFromNotifications() {
    if (!currentToken) {
        logActivity('No FCM token available', 'error');
        return;
    }

    const apiUrl = document.getElementById('apiUrl').value;
    const userId = parseInt(document.getElementById('userId').value);

    try {
        logActivity('Unsubscribing from notifications...', 'info');
        
        const response = await fetch(`${apiUrl}/api/subscription/unsubscribe`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userId: userId,
                deviceToken: currentToken
            })
        });

        const data = await response.json();
        
        if (response.ok && data.success) {
            logActivity('Successfully unsubscribed from notifications', 'success');
        } else {
            throw new Error(data.error || 'Unsubscription failed');
        }
    } catch (error) {
        logActivity(`Unsubscription failed: ${error.message}`, 'error');
    }
}

// Send test notification
async function sendTestNotification() {
    const apiUrl = document.getElementById('apiUrl').value;
    const userId = parseInt(document.getElementById('userId').value);

    try {
        logActivity('Sending test notification...', 'info');
        
        const response = await fetch(`${apiUrl}/api/notification/send-test?userIds=${userId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        const data = await response.json();
        
        if (response.ok && data.success) {
            logActivity('Test notification sent successfully', 'success');
        } else {
            throw new Error(data.error || 'Failed to send notification');
        }
    } catch (error) {
        logActivity(`Failed to send test notification: ${error.message}`, 'error');
    }
}

// Send custom notification
async function sendCustomNotification() {
    const apiUrl = document.getElementById('apiUrl').value;
    const userId = parseInt(document.getElementById('userId').value);
    const title = document.getElementById('notificationTitle').value;
    const body = document.getElementById('notificationBody').value;

    if (!title || !body) {
        logActivity('Please enter both title and body for the notification', 'error');
        return;
    }

    try {
        logActivity('Sending custom notification...', 'info');
        
        const response = await fetch(`${apiUrl}/api/notification/send`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                title: title,
                body: body,
                userIds: [userId],
                customData: {
                    source: 'web-client',
                    timestamp: new Date().toISOString()
                }
            })
        });

        const data = await response.json();
        
        if (response.ok && data.success) {
            logActivity('Custom notification sent successfully', 'success');
        } else {
            throw new Error(data.error || 'Failed to send notification');
        }
    } catch (error) {
        logActivity(`Failed to send custom notification: ${error.message}`, 'error');
    }
}

// Send notification to all users
async function sendToAllUsers() {
    const apiUrl = document.getElementById('apiUrl').value;
    const title = document.getElementById('notificationTitle').value;
    const body = document.getElementById('notificationBody').value;

    if (!title || !body) {
        logActivity('Please enter both title and body for the notification', 'error');
        return;
    }

    try {
        logActivity('Sending notification to all users...', 'info');
        
        const response = await fetch(`${apiUrl}/api/notification/send`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                title: title,
                body: body,
                sendToAll: true,
                customData: {
                    source: 'web-client-broadcast',
                    timestamp: new Date().toISOString()
                }
            })
        });

        const data = await response.json();
        
        if (response.ok && data.success) {
            logActivity('Broadcast notification sent successfully', 'success');
        } else {
            throw new Error(data.error || 'Failed to send notification');
        }
    } catch (error) {
        logActivity(`Failed to send broadcast notification: ${error.message}`, 'error');
    }
}

// Display received notification
function displayNotification(payload) {
    const notificationsList = document.getElementById('notificationsList');

    // Remove "no notifications" message if it exists
    const noNotificationsMsg = notificationsList.querySelector('.text-muted');
    if (noNotificationsMsg) {
        noNotificationsMsg.remove();
    }

    const notification = document.createElement('div');
    notification.className = 'card notification-card mb-2';

    const timestamp = new Date().toLocaleString();
    const title = payload.notification?.title || 'No Title';
    const body = payload.notification?.body || 'No Body';
    const data = payload.data ? JSON.stringify(payload.data, null, 2) : 'No custom data';

    notification.innerHTML = `
        <div class="card-body">
            <h6 class="card-title">${title}</h6>
            <p class="card-text">${body}</p>
            <small class="text-muted">Received: ${timestamp}</small>
            ${payload.data ? `
                <details class="mt-2">
                    <summary>Custom Data</summary>
                    <pre class="mt-2 p-2 bg-light rounded"><code>${data}</code></pre>
                </details>
            ` : ''}
        </div>
    `;

    notificationsList.insertBefore(notification, notificationsList.firstChild);

    // Show browser notification if permission is granted
    if (Notification.permission === 'granted') {
        new Notification(title, {
            body: body,
            icon: '/favicon.ico',
            tag: 'push-notification-test'
        });
    }
}

// Clear notifications list
function clearNotifications() {
    const notificationsList = document.getElementById('notificationsList');
    notificationsList.innerHTML = '<p class="text-muted">No notifications received yet...</p>';
    logActivity('Notifications list cleared', 'info');
}

// Log activity
function logActivity(message, type = 'info') {
    const activityLog = document.getElementById('activityLog');
    const timestamp = new Date().toLocaleTimeString();

    const logEntry = document.createElement('div');
    logEntry.className = `log-entry log-${type}`;

    const icon = getLogIcon(type);
    logEntry.innerHTML = `<i class="${icon}"></i> [${timestamp}] ${message}`;

    activityLog.insertBefore(logEntry, activityLog.firstChild);

    // Keep only last 100 entries
    while (activityLog.children.length > 100) {
        activityLog.removeChild(activityLog.lastChild);
    }

    // Auto-scroll to top
    activityLog.scrollTop = 0;
}

// Get icon for log type
function getLogIcon(type) {
    switch (type) {
        case 'success': return 'fas fa-check-circle text-success';
        case 'error': return 'fas fa-exclamation-circle text-danger';
        case 'warning': return 'fas fa-exclamation-triangle text-warning';
        case 'info':
        default: return 'fas fa-info-circle text-info';
    }
}

// Clear activity log
function clearLog() {
    const activityLog = document.getElementById('activityLog');
    activityLog.innerHTML = '';
    logActivity('Activity log cleared', 'info');
}

// Service Worker registration for background messages
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/firebase-messaging-sw.js')
        .then((registration) => {
            logActivity('Service Worker registered successfully', 'success');
        })
        .catch((error) => {
            logActivity(`Service Worker registration failed: ${error.message}`, 'error');
        });
}
