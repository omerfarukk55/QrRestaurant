using System.Collections.Generic;
using System.Linq;

namespace RestaurantQRSystem.ViewModels
{
    public class CartViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public string CustomerNote { get; set; }

        public decimal Total => Items.Sum(item => item.Total);
    }
}