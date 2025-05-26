using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dosyayonetim.api.Models
{
    public class DownloadHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public DateTime DownloadDate { get; set; }

        [Required]
        public string IpAddress { get; set; }

        [Required]
        public string UserAgent { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("FileId")]
        public virtual FileEntity File { get; set; }
    }
} 