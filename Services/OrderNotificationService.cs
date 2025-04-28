using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Hubs;
using RestaurantQRSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Services
{
    public class OrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderNotificationService> _logger;

        public OrderNotificationService(
            IHubContext<OrderHub> hubContext,
            ApplicationDbContext context,
            ILogger<OrderNotificationService> logger)
        {
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
        }

        // Yeni sipariş bildirimi gönderme
        public async Task SendNewOrderNotificationAsync(int orderId)
        {
            try
            {
                // Sipariş detaylarını veritabanından alıyoruz
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order not found for notification: {orderId}");
                    return;
                }

                // Bildirim detaylarını hazırlıyoruz
                var notification = new
                {
                    OrderId = order.Id,
                    TableId = order.TableId,
                    TableName = order.Table.Name,
                    CustomerName = order.CustomerName ?? "Misafir",
                    TotalAmount = order.TotalAmount,
                    ItemCount = order.OrderItems.Sum(i => i.Quantity),
                    OrderDate = order.OrderDate,
                    FormattedDate = order.OrderDate.ToString("HH:mm")
                };

                // SignalR üzerinden admin grubuna bildirim gönderiyoruz
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNewOrder", notification);
                _logger.LogInformation($"Order notification sent for Order #{orderId} to Admin group");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order notification for Order #{orderId}");
            }
        }

        // Sipariş durumu güncelleme bildirimi gönderme
        public async Task SendOrderStatusUpdateAsync(int orderId, string status)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("OrderStatusUpdated", orderId, status);
                _logger.LogInformation($"Order status update notification sent: Order #{orderId}, Status={status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending status update notification for Order #{orderId}");
            }
        }
    }
}