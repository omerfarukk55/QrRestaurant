using RestaurantQRSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Masa bilgisi gereklidir.")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Sepet bilgisi gereklidir.")]
        public string CartJson { get; set; }

        [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Lütfen notunuzu giriniz.")]
        public string CustomerNote { get; set; }
        public OrderStatus Status { get; internal set; }
        public DateTime OrderDate { get; set; }
        public int TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    }
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; internal set; }
    }
}