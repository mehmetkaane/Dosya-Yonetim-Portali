using System.ComponentModel.DataAnnotations;

namespace dosyayonetim.api.Models.DTOs
{
    public class CreateShareLinkDto
    {
        public int? Days { get; set; }
    }

    public class ShareLinkDto
    {
        public int Id { get; set; }
        public string ShareCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int DownloadCount { get; set; }
        public string FileName { get; set; }
    }
} 