using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantQRSystem.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string TableName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNote { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
        public Stream CartJson { get; internal set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Note { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }
}