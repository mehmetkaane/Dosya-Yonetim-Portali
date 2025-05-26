using System.ComponentModel.DataAnnotations;

namespace dosyayonetim.api.Models
{
    public class FileShareLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public string ShareCode { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        public int? DownloadCount { get; set; }

        public virtual FileEntity File { get; set; }
    }
} 