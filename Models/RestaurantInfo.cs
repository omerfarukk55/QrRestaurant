namespace RestaurantQRSystem.Models
{
    public class RestaurantInfo
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LogoUrl { get; set; }
        // Diğer ayar/profil alanları eklenebilir
    }
}