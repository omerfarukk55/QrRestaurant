using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.ViewModels
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Masa bilgisi gereklidir.")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Sepet bilgisi gereklidir.")]
        public string CartJson { get; set; }

        [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Lütfen notunuzu giriniz.")]
        public string CustomerNote { get; set; }
    }
}