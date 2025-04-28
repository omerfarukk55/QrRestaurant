namespace RestaurantQRSystem.ViewModels
{
    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
        public int CancelledCount { get; set; }
        public decimal CancelledAmount { get; set; }
    }
}
