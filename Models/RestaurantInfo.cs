using System.ComponentModel.DataAnnotations;

public class RestaurantInfo
{
    public int Id { get; set; }

    [Required]
    public string RestaurantName { get; set; }

    public string Description { get; set; }

    public string Address { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }

    public string? LogoUrl { get; set; }

    public string FacebookUrl { get; set; }

    public string InstagramUrl { get; set; }

    public bool ShowLogo { get; set; } = true;

    public string WorkingHours { get; set; }

    public string TaxNumber { get; set; }

    public string Currency { get; set; } = "₺";

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}