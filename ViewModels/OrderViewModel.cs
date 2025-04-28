using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.ViewModels
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Masa bilgisi gereklidir.")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Sepet bilgisi gereklidir.")]
        public string CartJson { get; set; }

        public string CustomerName { get; set; }

        public string CustomerNote { get; set; }
    }
}