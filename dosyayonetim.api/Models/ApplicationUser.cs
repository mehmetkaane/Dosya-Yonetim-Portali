using Microsoft.AspNetCore.Identity;

namespace dosyayonetim.api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
} 