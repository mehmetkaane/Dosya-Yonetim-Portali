namespace dosyayonetim.api.Models.DTOs
{
    public class StatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalFiles { get; set; }
        public int TotalShareLinks { get; set; }
        public int ActiveShareLinks { get; set; }
        public int TotalDownloads { get; set; }
        public List<FileTypeStat> FileTypeStats { get; set; }
        public List<DailyStat> DailyStats { get; set; }
    }

    public class FileTypeStat
    {
        public string FileType { get; set; }
        public int Count { get; set; }
    }

    public class DailyStat
    {
        public DateTime Date { get; set; }
        public int NewFiles { get; set; }
        public int NewShareLinks { get; set; }
        public int Downloads { get; set; }
    }
} 