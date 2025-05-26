namespace dosyayonetim.api.Models.DTOs
{
    public class FileDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
        public string? Description { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
        public string FormattedSize { get; set; }
    }

    public class CreateFileDto
    {
        public IFormFile File { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateFileDto
    {
        public IFormFile? File { get; set; }
        public string? Description { get; set; }
    }
} 