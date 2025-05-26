namespace dosyayonetim.api.Models.DTOs
{
    public class DownloadHistoryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public DateTime DownloadDate { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    public class DownloadStatisticsDto
    {
        public int TotalDownloads { get; set; }
        public int UniqueUsers { get; set; }
        public int UniqueFiles { get; set; }
        public List<DownloadHistoryDto> RecentDownloads { get; set; }
        public Dictionary<string, int> DownloadsByFileType { get; set; }
        public Dictionary<string, int> DownloadsByUser { get; set; }
    }
} 