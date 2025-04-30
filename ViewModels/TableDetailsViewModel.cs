using RestaurantQRSystem.Models;

namespace RestaurantQRSystem.ViewModels
{
    public class TableDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public string QRCode { get; set; }
        public Order CurrentOrder { get; set; }
    }
}
