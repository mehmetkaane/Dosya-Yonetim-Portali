using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dosyayonetim.api.Models.DTOs;
using dosyayonetim.api.Services;
using System.Security.Claims;
using dosyayonetim.api.Extensions;
using Microsoft.AspNetCore.Identity;
using dosyayonetim.api.Models;
using System.Globalization;

namespace dosyayonetim.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileShareController : ControllerBase
    {
        private readonly FileShareService _fileShareService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DownloadHistoryService _downloadHistoryService;

        public FileShareController(
            FileShareService fileShareService, 
            UserManager<ApplicationUser> userManager,
            DownloadHistoryService downloadHistoryService)
        {
            _fileShareService = fileShareService;
            _userManager = userManager;
            _downloadHistoryService = downloadHistoryService;
        }

        [Authorize]
        [HttpPost("files/{fileId}/share")]
        public async Task<ActionResult<ShareLinkDto>> CreateShareLink(int fileId, [FromBody] CreateShareLinkDto createShareLinkDto)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");


                DateTime? expiryDate = null;
                if (createShareLinkDto.Days.HasValue && createShareLinkDto.Days.Value > 0)
                {
                    expiryDate = DateTime.Now.AddDays(createShareLinkDto.Days.Value);
                }

                var shareLink = await _fileShareService.CreateShareLinkAsync(fileId, userId.ToString(), expiryDate);
                return Ok(shareLink);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Paylaşım linki oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("links")]
        public async Task<ActionResult<IEnumerable<ShareLinkDto>>> GetUserShareLinks()
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var shareLinks = await _fileShareService.GetUserShareLinksAsync(userId.ToString());
                
                // Tarihleri Türkçe formatına çevir
                var turkishCulture = new CultureInfo("tr-TR");
                foreach (var link in shareLinks)
                {
                    link.CreatedDate = DateTime.Parse(link.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss"), turkishCulture);
                    if (link.ExpiryDate.HasValue)
                    {
                        link.ExpiryDate = DateTime.Parse(link.ExpiryDate.Value.ToString("dd.MM.yyyy HH:mm:ss"), turkishCulture);
                    }
                }

                return Ok(shareLinks);
            }
            catch (Exception ex)
            {
                return BadRequest($"Paylaşım linkleri listelenirken bir hata oluştu: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("links/{shareLinkId}")]
        public async Task<IActionResult> DeactivateShareLink(int shareLinkId)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                await _fileShareService.DeactivateShareLinkAsync(shareLinkId, userId.ToString());
                return Ok("Başarıyla Link Paylaşımı kapatıld");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Paylaşım linki deaktif edilirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("download/{shareCode}")]
        public async Task<IActionResult> DownloadSharedFile(string shareCode)
        {
            try
            {
                var (fileBytes, fileName, contentType, fileId, userId) = await _fileShareService.DownloadSharedFileAsync(shareCode);

                // Eğer kullanıcı giriş yapmışsa indirme geçmişini kaydet
                if (User.Identity.IsAuthenticated)
                {
                    var currentUserId = await _userManager.FindByNameAsync(User.GetUsername());
                    if (currentUserId != null)
                    {
                        await _downloadHistoryService.RecordDownloadAsync(
                            currentUserId.Id,
                            fileId,
                            Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                            Request.Headers["User-Agent"].ToString()
                        );
                    }
                }
                else
                {
                    // Giriş yapmamış kullanıcılar için "anonymous" kullanıcı ID'si ile kayıt
                    await _downloadHistoryService.RecordDownloadAsync(
                        "anonymous",
                        fileId,
                        Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Request.Headers["User-Agent"].ToString()
                    );
                }

                return File(fileBytes, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound($"Dosya bulunamadı: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosya indirilirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("check/{shareCode}")]
        public async Task<IActionResult> CheckSharedFile(string shareCode)
        {
            try
            {
                var result = await _fileShareService.CheckSharedFileAsync(shareCode);
                return Ok(new { exists = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { exists = false, message = ex.Message });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { exists = false, message = "Dosya bulunamadı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exists = false, message = "Dosya kontrolü sırasında bir hata oluştu" });
            }
        }
    }
} 