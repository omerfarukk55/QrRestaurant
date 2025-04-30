namespace RestaurantQRSystem.ViewModels
{
    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public double TotalSales { get; set; }
        public int CancelledCount { get; set; }
        public double CancelledAmount { get; set; }
    }
}
