namespace RestaurantQRSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int TotalTables { get; set; }
        public int NewOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
    }
}