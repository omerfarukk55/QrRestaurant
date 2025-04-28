using RestaurantQRSystem.Models;

public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; }
    public string CustomerName { get; set; }

    // Navigation property
    public virtual Order Order { get; set; }
}