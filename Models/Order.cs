using RestaurantQRSystem.Models.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantQRSystem.Models
{
    public class Order
    {
        

        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNote { get; set; }

        public int TableId { get; set; }
        public virtual Table Table { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? PaymentNotes { get; set; }
    }
}