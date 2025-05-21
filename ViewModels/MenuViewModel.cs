using RestaurantQRSystem.Models;
using System.Collections.Generic;

namespace RestaurantQRSystem.ViewModels
{
    public class MenuViewModel
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public string TableName { get; set; }
        public List<Category> Categories { get; set; }
        public RestaurantInfo RestaurantInfo { get; set; }
    }
}