// Import Firebase scripts
importScripts('https://www.gstatic.com/firebasejs/9.0.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.0.0/firebase-messaging-compat.js');

// Initialize Firebase in service worker
// Note: You need to replace these with your actual Firebase config
const firebaseConfig = {
  apiKey: "your-api-key",
  authDomain: "your-project.firebaseapp.com",
  projectId: "your-project-id",
  storageBucket: "your-project.appspot.com",
  messagingSenderId: "your-sender-id",
  appId: "your-app-id"
};

firebase.initializeApp(firebaseConfig);

// Retrieve Firebase Messaging object
const messaging = firebase.messaging();

// Handle background messages
messaging.onBackgroundMessage(function(payload) {
  console.log('[firebase-messaging-sw.js] Received background message ', payload);
  
  // Customize notification here
  const notificationTitle = payload.notification?.title || 'Push Notification Test';
  const notificationOptions = {
    body: payload.notification?.body || 'You have a new message',
    icon: '/favicon.ico',
    badge: '/badge-icon.png',
    tag: 'push-notification-test',
    data: payload.data || {},
    actions: [
      {
        action: 'open',
        title: 'Open App',
        icon: '/open-icon.png'
      },
      {
        action: 'dismiss',
        title: 'Dismiss',
        icon: '/dismiss-icon.png'
      }
    ]
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});

// Handle notification click
self.addEventListener('notificationclick', function(event) {
  console.log('[firebase-messaging-sw.js] Notification click received.');

  event.notification.close();

  if (event.action === 'dismiss') {
    // User clicked dismiss, do nothing
    return;
  }

  // Handle notification click (open app)
  event.waitUntil(
    clients.matchAll({
      type: 'window',
      includeUncontrolled: true
    }).then(function(clientList) {
      // If app is already open, focus it
      for (let i = 0; i < clientList.length; i++) {
        const client = clientList[i];
        if (client.url.includes(self.location.origin) && 'focus' in client) {
          return client.focus();
        }
      }
      
      // If app is not open, open it
      if (clients.openWindow) {
        return clients.openWindow('/');
      }
    })
  );
});

// Handle push event (for additional processing)
self.addEventListener('push', function(event) {
  console.log('[firebase-messaging-sw.js] Push event received:', event);
  
  // Additional push event handling can be added here
  // This is called before onBackgroundMessage
});

// Handle service worker installation
self.addEventListener('install', function(event) {
  console.log('[firebase-messaging-sw.js] Service worker installing...');
  self.skipWaiting();
});

// Handle service worker activation
self.addEventListener('activate', function(event) {
  console.log('[firebase-messaging-sw.js] Service worker activating...');
  event.waitUntil(self.clients.claim());
});
