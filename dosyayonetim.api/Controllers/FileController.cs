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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DownloadHistoryService _downloadHistoryService;

        public FileController(FileService fileService, UserManager<ApplicationUser> userManager, DownloadHistoryService downloadHistoryService)
        {
            _fileService = fileService;
            _userManager = userManager;
            _downloadHistoryService = downloadHistoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileDto>>> GetAllFiles()
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var files = await _fileService.GetAllFilesAsync(userId.ToString());
                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosyalar listelenirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileDto>> GetFile(int id)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var file = await _fileService.GetFileByIdAsync(id, userId.ToString());
                if (file == null)
                    return NotFound($"ID: {id} olan dosya bulunamadı.");
                return Ok(file);
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosya bilgileri alınırken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<FileDto>> UploadFile([FromForm] CreateFileDto createFileDto)
        {
            try
            {
                if (createFileDto.File == null || createFileDto.File.Length == 0)
                    return BadRequest("Lütfen yüklenecek bir dosya seçin.");

                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var file = await _fileService.UploadFileAsync(createFileDto, userId.ToString());
                return CreatedAtAction(nameof(GetFile), new { id = file.Id }, file);
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosya yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FileDto>> UpdateFile(int id, [FromForm] UpdateFileDto updateFileDto)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var file = await _fileService.UpdateFileAsync(id, updateFileDto, userId.ToString());
                return Ok(file);
            }
            catch (ArgumentException ex)
            {
                return NotFound($"ID: {id} olan dosya bulunamadı.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosya güncellenirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                await _fileService.DeleteFileAsync(id, userId.ToString());
                return Ok("Başarılya Silindi");
            }
            catch (ArgumentException ex)
            {
                return NotFound($"ID: {id} olan dosya bulunamadı.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosya silinirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var (fileBytes, fileName, contentType) = await _fileService.DownloadFileAsync(id, userId.Id);

                // İndirme geçmişini kaydet
                await _downloadHistoryService.RecordDownloadAsync(
                    userId.Id,
                    id,
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return File(fileBytes, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                return NotFound($"ID: {id} olan dosya bulunamadı veya silinmiş olabilir.");
            }
            catch (FileNotFoundException ex)
            {
                return NotFound($"Dosya fiziksel olarak bulunamadı. Lütfen sistem yöneticisi ile iletişime geçin.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosya indirilirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("storage-info")]
        public async Task<IActionResult> GetUserStorageInfo()
        {
            try
             {
                var userId = await _userManager.FindByNameAsync(User.GetUsername());
                if (userId == null)
                    return BadRequest("Kullanıcı bilgileri alınamadı.");

                var (fileCount, totalSize) = await _fileService.GetUserStorageInfoAsync(User.GetUsername());
                return Ok(new
                {
                    FileCount = fileCount,
                    TotalSize = totalSize,
                    FormattedSize = FormatFileSize(totalSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Depolama bilgileri alınırken bir hata oluştu: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<ActionResult<IEnumerable<FileDto>>> GetAllFilesForAdmin()
        {
            try
            {
                var files = await _fileService.GetAllFilesForAdminAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest($"Dosyalar listelenirken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 