using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.ViewModels
{
    public class RestaurantSettingsViewModel
    {
        [Required(ErrorMessage = "Restoran adı gereklidir.")]
        [Display(Name = "Restoran Adı")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Restoran adı 2-100 karakter arasında olmalıdır.")]
        public string RestaurantName { get; set; }

        [Display(Name = "Restoran Açıklaması")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Description { get; set; }

        [Display(Name = "Adres")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string Address { get; set; }

        [Display(Name = "Telefon")]
        [RegularExpression(@"^[0-9\s\+\-$$$$]+$", ErrorMessage = "Geçerli bir telefon numarası girin.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; }

        public string? CurrentLogoPath { get; set; }

        [Display(Name = "Logo Yükle")]
        public IFormFile LogoFile { get; set; }

        [Display(Name = "Sosyal Medya - Facebook")]
        public string FacebookUrl { get; set; }

        [Display(Name = "Sosyal Medya - Instagram")]
        public string InstagramUrl { get; set; }

        [Display(Name = "Göster/Gizle")]
        public bool ShowLogo { get; set; } = true;

        [Display(Name = "Çalışma Saatleri")]
        public string WorkingHours { get; set; }

        [Display(Name = "Vergi Numarası")]
        public string TaxNumber { get; set; }

        [Display(Name = "Para Birimi")]
        public string Currency { get; set; } = "₺";
    }
}