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
    [Authorize(Roles = "Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsService _statisticsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(StatisticsService statisticsService, UserManager<ApplicationUser> userManager)
        {
            _statisticsService = statisticsService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<StatisticsDto>> GetStatistics()
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var user = await _userManager.FindByNameAsync(User.GetUsername());
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    return Forbid("Bu işlem için admin yetkisi gereklidir.");

                var statistics = await _statisticsService.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"İstatistikler alınırken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 