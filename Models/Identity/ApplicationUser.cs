using Microsoft.AspNetCore.Identity;
using System;

namespace RestaurantQRSystem.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}