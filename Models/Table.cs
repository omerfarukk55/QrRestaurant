using RestaurantQRSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantQRSystem.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string QrCode { get; set; }

        // Bu alanları kaldırıyoruz
        // public bool IsOccupied { get; set; } 
        // public DateTime? OccupiedSince { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        // TableStatus'u hesaplayan property ekleyelim
        public TableStatus Status
        {
            get
            {
                if (Orders == null || !Orders.Any()) return TableStatus.Available;

                // Aktif siparişleri kontrol et (Tamamlanmamış veya iptal edilmemiş)
                bool hasActiveOrders = Orders.Any(o =>
                    o.Status != OrderStatus.Completed &&
                    o.Status != OrderStatus.Cancelled &&
                    o.Status != OrderStatus.Paid);

                return hasActiveOrders ? TableStatus.Occupied : TableStatus.Available;
            }
        }

        public bool IsOccupied { get; internal set; }
        public DateTime? OccupiedSince { get; set; }

        // Aktif siparişleri getiren bir method ekleyelim
        public IEnumerable<Order> GetActiveOrders()
        {
            if (Orders == null) return new List<Order>();

            return Orders.Where(o => o.Status != OrderStatus.Completed &&
                                  o.Status != OrderStatus.Cancelled &&
                                  o.Status != OrderStatus.Paid);
        }
    }
}