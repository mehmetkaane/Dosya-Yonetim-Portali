using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dosyayonetim.api.Models.DTOs;
using dosyayonetim.api.Services;
using System.Security.Claims;
using dosyayonetim.api.Extensions;
using Microsoft.AspNetCore.Identity;
using dosyayonetim.api.Models;

namespace dosyayonetim.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadHistoryController : ControllerBase
    {
        private readonly DownloadHistoryService _downloadHistoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DownloadHistoryController(
            DownloadHistoryService downloadHistoryService,
            UserManager<ApplicationUser> userManager)
        {
            _downloadHistoryService = downloadHistoryService;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<DownloadHistoryDto>>> GetUserDownloadHistory()
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                {
                    return BadRequest("Kullanıcı bilgileri alınamadı.");
                }

                var history = await _downloadHistoryService.GetUserDownloadHistoryAsync(userId.Id);
                
                if (!history.Any())
                {
                    return Ok(new List<DownloadHistoryDto>()); // Boş liste döndür
                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                // Hata loglaması yapılabilir
                return BadRequest($"İndirme geçmişi alınırken bir hata oluştu: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("statistics")]
        public async Task<ActionResult<DownloadStatisticsDto>> GetDownloadStatistics()
        {
            try
            {
                var statistics = await _downloadHistoryService.GetDownloadStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"İndirme istatistikleri alınırken bir hata oluştu: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<DownloadHistoryDto>>> GetAllRecentDownloads()
        {
            try
            {
                var downloads = await _downloadHistoryService.GetAllRecentDownloadsAsync();
                return Ok(downloads);
            }
            catch (Exception ex)
            {
                return BadRequest($"Son indirmeler alınırken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 