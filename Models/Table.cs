namespace RestaurantQRSystem.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string QrCode { get; set; }
        public bool IsOccupied { get; set; } 
        public DateTime? OccupiedSince { get; set; }
    }
}