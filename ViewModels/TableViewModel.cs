namespace RestaurantQRSystem.ViewModels
{
    public class TableViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public int? CurrentOrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public object ActiveOrders { get; internal set; }
        public object Status { get; internal set; }
    }
}
