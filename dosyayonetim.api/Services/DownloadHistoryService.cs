using Microsoft.EntityFrameworkCore;
using dosyayonetim.api.Data;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.DTOs;
using System.Linq;

namespace dosyayonetim.api.Services
{
    public class DownloadHistoryService
    {
        private readonly ApplicationDbContext _context;

        public DownloadHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DownloadHistoryDto> RecordDownloadAsync(string userId, int fileId, string ipAddress, string userAgent)
        {
            var effectiveUserId = string.IsNullOrEmpty(userId) ? "anonymous" : userId;

            var download = new DownloadHistory
            {
                UserId = effectiveUserId,
                FileId = fileId,
                DownloadDate = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.DownloadHistories.Add(download);
            await _context.SaveChangesAsync();

            return await GetDownloadHistoryDtoAsync(download.Id);


        }

        public async Task<DownloadHistoryDto> GetDownloadHistoryDtoAsync(int id)
        {
            var download = await _context.DownloadHistories
                .Include(d => d.File)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (download == null)
                return null;

            var userName = "Anonim";
            if (download.UserId != "anonymous")
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == download.UserId);
                if (user != null)
                {
                    userName = user.UserName;
                }
            }

            return new DownloadHistoryDto
            {
                Id = download.Id,
                UserId = download.UserId,
                UserName = userName,
                FileId = download.FileId,
                FileName = download.File.FileName,
                DownloadDate = download.DownloadDate,
                IpAddress = download.IpAddress,
                UserAgent = download.UserAgent
            };
        }

        public async Task<DownloadStatisticsDto> GetDownloadStatisticsAsync()
        {
            var downloads = await _context.DownloadHistories
                .Include(d => d.User)
                .Include(d => d.File)
                .ToListAsync();

            var statistics = new DownloadStatisticsDto
            {
                TotalDownloads = downloads.Count,
                UniqueUsers = downloads.Select(d => d.UserId).Distinct().Count(),
                UniqueFiles = downloads.Select(d => d.FileId).Distinct().Count(),
                DownloadsByFileType = downloads
                    .GroupBy(d => Path.GetExtension(d.File.FileName).ToLower())
                    .ToDictionary(g => g.Key, g => g.Count()),
                DownloadsByUser = downloads
                    .GroupBy(d => d.User.UserName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentDownloads = downloads
                    .OrderByDescending(d => d.DownloadDate)
                    .Take(10)
                    .Select(d => new DownloadHistoryDto
                    {
                        Id = d.Id,
                        UserName = d.User.UserName,
                        FileName = d.File.FileName,
                        DownloadDate = d.DownloadDate,
                        IpAddress = d.IpAddress,
                        UserAgent = d.UserAgent
                    })
                    .ToList()
            };

            return statistics;
        }

        public async Task<IEnumerable<DownloadHistoryDto>> GetUserDownloadHistoryAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Enumerable.Empty<DownloadHistoryDto>();

            return await _context.DownloadHistories
                .Include(d => d.File)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DownloadDate)
                .Select(d => new DownloadHistoryDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    UserName = user.UserName,
                    FileId = d.FileId,
                    FileName = d.File != null ? d.File.FileName : "Silinmiş Dosya",
                    DownloadDate = d.DownloadDate,
                    IpAddress = d.IpAddress,
                    UserAgent = d.UserAgent
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DownloadHistoryDto>> GetAllRecentDownloadsAsync()
        {
            var downloads = await _context.DownloadHistories
                .Include(d => d.User)
                .Include(d => d.File)
                .OrderByDescending(d => d.DownloadDate)
                .Take(50) // Son 50 indirmeyi getir
                .Select(d => new DownloadHistoryDto
                {
                    Id = d.Id,
                    UserName = d.User.UserName,
                    FileName = d.File.FileName,
                    DownloadDate = d.DownloadDate,
                    IpAddress = d.IpAddress,
                    UserAgent = d.UserAgent
                })
                .ToListAsync();

            return downloads;
        }
    }
} 