using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        public async Task NotifyNewOrder(int orderId, string tableName)
        {
            await Clients.Group("Admins").SendAsync("ReceiveNewOrder", orderId, tableName);
        }

        public async Task UpdateOrderStatus(int orderId, string status)
        {
            await Clients.All.SendAsync("OrderStatusUpdated", orderId, status);
        }
    }
}