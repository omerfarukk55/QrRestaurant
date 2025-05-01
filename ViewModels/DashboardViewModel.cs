using RestaurantQRSystem.Models;

namespace RestaurantQRSystem.ViewModels
{
    public class DashboardViewModel
    {
        public AdminDashboardViewModel AdminStats { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<Order> ActiveOrders { get; set; }
        public List<TableDetailsViewModel> TableList { get; set; }
    }
}
