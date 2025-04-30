namespace RestaurantQRSystem.ViewModels
{
    public class TableStatusViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOccupied { get; set; }
        public int? CurrentOrderId { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public decimal TotalAmount { get; set; }
        public bool ReadyForPayment { get; set; }
        public string QrCodeUrl { get; set; }
    }
}
