using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Hubs
{
    public class OrderHub : Hub
    {
        private readonly ILogger<OrderHub> _logger;

        public OrderHub(ILogger<OrderHub> logger)
        {
            _logger = logger;
        }

        // Admin grubuna katılma metodu
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined Admin group");
        }

        // Sipariş durumu güncelleme
        public async Task UpdateOrderStatus(int orderId, string status)
        {
            await Clients.All.SendAsync("OrderStatusUpdated", orderId, status);
            _logger.LogInformation($"Order status updated: OrderId={orderId}, Status={status}");
        }

        // Bağlantı kurulduğunda
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // Bağlantı kesildiğinde
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
        public async Task NotifyNewOrder(int orderId, string tableName, decimal totalAmount)
        {
            var notification = new
            {
                orderId,
                tableName,
                totalAmount,
                formattedDate = DateTime.Now.ToString("HH:mm")
            };

            await Clients.Group("Admins").SendAsync("ReceiveNewOrder", notification);
            _logger.LogInformation($"New order notification sent: OrderId={orderId}, Table={tableName}, Amount={totalAmount}");
        }
    }
}