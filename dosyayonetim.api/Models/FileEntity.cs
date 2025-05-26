using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dosyayonetim.api.Models
{
    public class FileEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public string FilePath { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [Required]
        public string UploadedBy { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }
    }
} 