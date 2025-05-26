using AutoMapper;
using dosyayonetim.api.Data;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace dosyayonetim.api.Services
{
    public class FileShareService
    {
        private readonly GenericRepository<FileShareLink> _shareLinkRepository;
        private readonly GenericRepository<FileEntity> _fileRepository;
        private readonly IMapper _mapper;

        public FileShareService(
            GenericRepository<FileShareLink> shareLinkRepository,
            GenericRepository<FileEntity> fileRepository,
            IMapper mapper)
        {
            _shareLinkRepository = shareLinkRepository;
            _fileRepository = fileRepository;
            _mapper = mapper;
        }

        public async Task<ShareLinkDto> CreateShareLinkAsync(int fileId, string userId, DateTime? expiryDate)
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null)
                throw new ArgumentException("Dosya bulunamadı.");

            if (file.UploadedBy != userId)
                throw new ArgumentException("Bu dosya üzerinde işlem yapma yetkiniz yok.");

            var shareLink = new FileShareLink
            {
                FileId = fileId,
                ShareCode = GenerateShareCode(),
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                ExpiryDate = expiryDate,
                IsActive = true,
                DownloadCount = 0
            };

            await _shareLinkRepository.AddAsync(shareLink);
            return _mapper.Map<ShareLinkDto>(shareLink);
        }

        public async Task<IEnumerable<ShareLinkDto>> GetUserShareLinksAsync(string userId)
        {
            var shareLinks = await _shareLinkRepository.FindAsync(s => s.CreatedBy == userId);
            return _mapper.Map<IEnumerable<ShareLinkDto>>(shareLinks);
        }

        public async Task<ShareLinkDto> GetShareLinkByCodeAsync(string shareCode)
        {
            var shareLinks = await _shareLinkRepository.FindAsync(s => s.ShareCode == shareCode && s.IsActive);
            var shareLink = shareLinks.FirstOrDefault();

            if (shareLink == null)
                throw new ArgumentException("Share link not found or is inactive.");

            if (shareLink.ExpiryDate.HasValue && shareLink.ExpiryDate.Value < DateTime.UtcNow)
            {
                shareLink.IsActive = false;
                await _shareLinkRepository.UpdateAsync(shareLink);
                throw new ArgumentException("Share link has expired.");
            }

            return _mapper.Map<ShareLinkDto>(shareLink);
        }

        public async Task<(byte[] fileBytes, string fileName, string contentType, int fileId, string userId)> DownloadSharedFileAsync(string shareCode)
        {
            var shareLink = await _shareLinkRepository.FindAsync(s => s.ShareCode == shareCode && s.IsActive);
            var shareLinkEntity = shareLink.FirstOrDefault();

            if (shareLinkEntity == null)
                throw new ArgumentException("Geçersiz veya süresi dolmuş paylaşım linki.");

            if (shareLinkEntity.ExpiryDate.HasValue && shareLinkEntity.ExpiryDate.Value < DateTime.UtcNow)
            {
                shareLinkEntity.IsActive = false;
                await _shareLinkRepository.UpdateAsync(shareLinkEntity);
                throw new ArgumentException("Paylaşım linkinin süresi dolmuş.");
            }

            var file = await _fileRepository.GetByIdAsync(shareLinkEntity.FileId);
            if (file == null)
                throw new ArgumentException("Dosya bulunamadı.");

            if (!System.IO.File.Exists(file.FilePath))
                throw new FileNotFoundException("Dosya bulunamadı.");

            // Update download count
            shareLinkEntity.DownloadCount = (shareLinkEntity.DownloadCount ?? 0) + 1;
            await _shareLinkRepository.UpdateAsync(shareLinkEntity);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(file.FilePath);
            return (fileBytes, file.FileName, file.ContentType, file.Id, file.UploadedBy);
        }

        public async Task DeactivateShareLinkAsync(int shareLinkId, string userId)
        {
            var shareLinks = await _shareLinkRepository.FindAsync(s => s.Id == shareLinkId && s.CreatedBy == userId);
            var shareLink = shareLinks.FirstOrDefault();

            if (shareLink == null)
                throw new ArgumentException("Share link not found or you don't have permission to deactivate it.");

            shareLink.IsActive = false;
            await _shareLinkRepository.UpdateAsync(shareLink);
        }

        public async Task<bool> CheckSharedFileAsync(string shareCode)
        {
            var shareLink = await _shareLinkRepository.FindAsync(s => s.ShareCode == shareCode && s.IsActive);
            var shareLinkEntity = shareLink.FirstOrDefault();

            if (shareLinkEntity == null)
                throw new ArgumentException("Geçersiz veya süresi dolmuş paylaşım linki.");

            if (shareLinkEntity.ExpiryDate.HasValue && shareLinkEntity.ExpiryDate.Value < DateTime.UtcNow)
            {
                shareLinkEntity.IsActive = false;
                await _shareLinkRepository.UpdateAsync(shareLinkEntity);
                throw new ArgumentException("Paylaşım linkinin süresi dolmuş.");
            }

            var file = await _fileRepository.GetByIdAsync(shareLinkEntity.FileId);
            if (file == null)
                throw new FileNotFoundException("Dosya bulunamadı.");

            if (!System.IO.File.Exists(file.FilePath))
                throw new FileNotFoundException("Dosya bulunamadı.");

            return true;
        }

        private string GenerateShareCode()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .TrimEnd('=');
            }
        }
    }
} 