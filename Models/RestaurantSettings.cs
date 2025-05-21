using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.Models
{
    public class RestaurantSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RestaurantName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string LogoPath { get; set; }

        public string FacebookUrl { get; set; }

        public string InstagramUrl { get; set; }

        public bool ShowLogo { get; set; } = true;

        public string WorkingHours { get; set; }

        public string TaxNumber { get; set; }

        public string Currency { get; set; } = "₺";

        public string ThemeColor { get; set; } = "#007bff";

        public string FooterText { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Phone { get; internal set; }
        public string LogoUrl { get; internal set; }
    }
}