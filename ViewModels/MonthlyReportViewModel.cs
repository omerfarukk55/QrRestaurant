namespace RestaurantQRSystem.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<DailySalesViewModel> DailySales { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
    }
}
