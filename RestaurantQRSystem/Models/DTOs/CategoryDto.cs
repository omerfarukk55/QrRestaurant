using System.ComponentModel.DataAnnotations;

namespace RestaurantQRSystem.Models.DTOs
{
    public class CategoryDto
    {
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Sıra zorunludur.")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
