namespace RestaurantQRSystem.Models.Identity.ViewModels
{
    public class loginViewModel
    {
        public Models.ApplicationUser Email { get; internal set; }
        public string Password { get; internal set; }
        public bool RememberMe { get; internal set; }
    }
}
