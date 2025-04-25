using System.Collections.Generic;

namespace RestaurantQRSystem.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; }
    }
}