using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantQRSystem.ViewModels
{
    public class TableDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TableStatus Status { get; set; }
        public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
        public int TotalAmount { get; set; }

        // Object yerine List<OrderItemViewModel> kullanın
        public List<TableOrderItemViewModel> OrderItems { get; set; } = new List<TableOrderItemViewModel>();

        public int? CurrentOrderId { get; set; }

        // Object yerine DateTime? kullanın
        public DateTime? OccupiedSince { get; set; }

        public bool IsOccupied { get; set; }

        // Table modelinden ViewModel'e dönüştüren metod
        public static TableDetailsViewModel FromTable(Table table)
        {
            // Başlangıçta Debug bilgisi
            System.Diagnostics.Debug.WriteLine($"FromTable - Masa {table.Id} için dönüşüm başladı");
            System.Diagnostics.Debug.WriteLine($"FromTable - Masa siparişleri: {table.Orders?.Count ?? 0}");

            // Aktif siparişleri filtrele
            var activeOrders = table.Orders?
                .Where(o => o.Status != OrderStatus.Completed &&
                          o.Status != OrderStatus.Cancelled &&
                          o.Status != OrderStatus.Paid)
                .ToList() ?? new List<Order>();

            System.Diagnostics.Debug.WriteLine($"FromTable - Aktif sipariş sayısı: {activeOrders.Count}");

            var viewModel = new TableDetailsViewModel
            {
                Id = table.Id,
                Name = table.Name,
                Status = table.Status,
                IsOccupied = table.IsOccupied,
                OccupiedSince = table.OccupiedSince,

                // Aktif siparişlerden OrderViewModels oluştur
                Orders = activeOrders.Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    TableId = o.TableId,
                    CustomerName = o.CustomerName,
                    CustomerNote = o.CustomerNote,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    TotalAmount = (int)o.TotalAmount,
                    OrderItems = o.OrderItems?.Select(oi => new OrderItemViewModel
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? "Ürün",
                        Quantity = (int)oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList() ?? new List<OrderItemViewModel>()
                }).ToList()
            };

            System.Diagnostics.Debug.WriteLine($"FromTable - Dönüşüm tamamlandı, ViewModel siparişleri: {viewModel.Orders.Count}");

            return viewModel;
        }
    }

    public class TableOrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}