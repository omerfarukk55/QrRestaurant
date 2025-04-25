namespace RestaurantQRSystem.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public decimal Total => Price * Quantity;
    }
}