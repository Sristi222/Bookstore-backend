using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Try_application.Hubs
{
    public class CustomerNotificationHub : Hub
    {
        // Static list to store recent notifications (in-memory storage)
        private static readonly List<string> _recentNotifications = new List<string>();
        private static readonly int _maxNotifications = 10; // Store last 10 notifications
        private static readonly object _lock = new object(); // For thread safety

        // When a new client connects, send them the recent notifications
        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            Console.WriteLine($"⭐ New client connected: {connectionId}");

            // Send recent notifications to the new client
            if (_recentNotifications.Count > 0)
            {
                Console.WriteLine($"⭐ Sending {_recentNotifications.Count} recent notifications to client {connectionId}");
                await Clients.Client(connectionId).SendAsync("ReceiveNotificationHistory", _recentNotifications);
            }
            else
            {
                Console.WriteLine($"⭐ No recent notifications to send to client {connectionId}");
            }

            await base.OnConnectedAsync();
        }

        // Public method to add a notification to history
        public static void AddNotificationToHistory(string message)
        {
            lock (_lock) // Thread safety for static collection
            {
                Console.WriteLine($"⭐ Adding notification to history: {message}");

                // Add new notification
                _recentNotifications.Add(message);

                // Keep only the most recent notifications
                while (_recentNotifications.Count > _maxNotifications)
                {
                    _recentNotifications.RemoveAt(0);
                }

                Console.WriteLine($"⭐ Current notification history count: {_recentNotifications.Count}");
            }
        }

        // For debugging - get current notification count
        public static int GetNotificationCount()
        {
            return _recentNotifications.Count;
        }
    }
}