namespace RestaurantQRSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Mevcut doğrudan özellikler
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int TotalTables { get; set; }
        public int NewOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TodayOrders { get; set; }
        public double TodayRevenue { get; set; }
        public List<TableDetailsViewModel> TableList { get; set; }

        // View'da kullanılan AdminStats özelliği için
        public AdminStatsViewModel AdminStats => new AdminStatsViewModel
        {
            TotalCategories = this.TotalCategories,
            TotalProducts = this.TotalProducts,
            TotalTables = this.TotalTables,
            NewOrders = this.NewOrders,
            ProcessingOrders = this.ProcessingOrders,
            CompletedOrders = this.CompletedOrders,
            TodayOrders = this.TodayOrders,
            TodayRevenue = this.TodayRevenue
        };
    }

    // AdminStats yapısı (halihazırda tanımlı değilse)
    public class AdminStatsViewModel
    {
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int TotalTables { get; set; }
        public int NewOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TodayOrders { get; set; }
        public double TodayRevenue { get; set; }
    }


}