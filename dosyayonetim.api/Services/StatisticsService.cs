using dosyayonetim.api.Data;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dosyayonetim.api.Services
{
    public class StatisticsService
    {
        private readonly GenericRepository<ApplicationUser> _userRepository;
        private readonly GenericRepository<FileEntity> _fileRepository;
        private readonly GenericRepository<FileShareLink> _shareLinkRepository;

        public StatisticsService(
            GenericRepository<ApplicationUser> userRepository,
            GenericRepository<FileEntity> fileRepository,
            GenericRepository<FileShareLink> shareLinkRepository)
        {
            _userRepository = userRepository;
            _fileRepository = fileRepository;
            _shareLinkRepository = shareLinkRepository;
        }

        public async Task<StatisticsDto> GetStatisticsAsync()
        {
            var stats = new StatisticsDto
            {
                TotalUsers = await _userRepository.CountAsync(),
                TotalFiles = await _fileRepository.CountAsync(f => !f.IsDeleted),
                TotalShareLinks = await _shareLinkRepository.CountAsync(),
                ActiveShareLinks = await _shareLinkRepository.CountAsync(s => s.IsActive),
                TotalDownloads = await _shareLinkRepository.GetAll()
                    .SumAsync(s => s.DownloadCount ?? 0),
                FileTypeStats = await GetFileTypeStatsAsync(),
                DailyStats = await GetDailyStatsAsync()
            };

            return stats;
        }

        private async Task<List<FileTypeStat>> GetFileTypeStatsAsync()
        {
            var files = await _fileRepository.GetAll()
                .Where(f => !f.IsDeleted)
                .ToListAsync();

            return files
                .GroupBy(f => f.ContentType)
                .Select(g => new FileTypeStat
                {
                    FileType = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

        private async Task<List<DailyStat>> GetDailyStatsAsync()
        {
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Now.AddDays(-i))
                .ToList();

            var dailyStats = new List<DailyStat>();

            foreach (var date in last30Days)
            {
                var startOfDay = date.Date;
                var endOfDay = date.Date.AddDays(1);

                var newFiles = await _fileRepository.CountAsync(f => 
                    f.UploadDate >= startOfDay && 
                    f.UploadDate < endOfDay && 
                    !f.IsDeleted);

                var newShareLinks = await _shareLinkRepository.CountAsync(s => 
                    s.CreatedDate >= startOfDay && 
                    s.CreatedDate < endOfDay);

                var downloads = await _shareLinkRepository.GetAll()
                    .Where(s => s.CreatedDate >= startOfDay && s.CreatedDate < endOfDay)
                    .SumAsync(s => s.DownloadCount ?? 0);

                dailyStats.Add(new DailyStat
                {
                    Date = date,
                    NewFiles = newFiles,
                    NewShareLinks = newShareLinks,
                    Downloads = downloads
                });
            }

            return dailyStats.OrderBy(d => d.Date).ToList();
        }
    }
} 