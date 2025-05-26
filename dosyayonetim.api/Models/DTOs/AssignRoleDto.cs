using System.ComponentModel.DataAnnotations;

namespace dosyayonetim.api.Models.DTOs
{
    public class AssignRoleDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
} 