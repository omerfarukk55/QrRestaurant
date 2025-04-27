using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RestaurantQRSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Sıra zorunludur.")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; }
    }
}