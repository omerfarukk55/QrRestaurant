using Newtonsoft.Json;
using RestaurantQRSystem.Models;

namespace RestaurantQRSystem.ViewModels
{
    public class TableDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public decimal TotalAmount { get; set; }
        public int? CurrentOrderId { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public Order? CurrentOrder { get; internal set; }
        public string QRCode { get; internal set; }
    }
    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CompleteOrderViewModel
    {
        public int TableId { get; set; }
        public int OrderId { get; set; }
    }
}
