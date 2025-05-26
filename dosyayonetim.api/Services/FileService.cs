using AutoMapper;
using dosyayonetim.api.Data;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.DTOs;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace dosyayonetim.api.Services
{
    public class FileService
    {
        private readonly GenericRepository<FileEntity> _repository;
        private readonly IMapper _mapper;
        private readonly string _uploadDirectory;
        private readonly ApplicationDbContext _context;

        public FileService(GenericRepository<FileEntity> repository, IMapper mapper, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _uploadDirectory = Path.Combine(environment.ContentRootPath, "Uploads");
            
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }

            _context = context;
        }

        public async Task<IEnumerable<FileDto>> GetAllFilesAsync(string userId)
        {
            var files = await _repository.FindAsync(f => f.UploadedBy == userId && !f.IsDeleted);
            return _mapper.Map<IEnumerable<FileDto>>(files);
        }

        public async Task<FileDto> GetFileByIdAsync(int id, string userId)
        {
            var file = await _repository.FindAsync(f => f.Id == id && f.UploadedBy == userId && !f.IsDeleted);
            var fileEntity = file.FirstOrDefault();
            return _mapper.Map<FileDto>(fileEntity);
        }

        public async Task<FileDto> UploadFileAsync(CreateFileDto createFileDto, string userId)
        {
            var file = createFileDto.File;
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was uploaded.");

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_uploadDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileEntity = new FileEntity
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = filePath,
                Description = createFileDto.Description,
                UploadDate = DateTime.UtcNow,
                UploadedBy = userId
            };

            await _repository.AddAsync(fileEntity);
            return _mapper.Map<FileDto>(fileEntity);
        }

        public async Task<FileDto> UpdateFileAsync(int id, UpdateFileDto updateFileDto, string userId)
        {
            var files = await _repository.FindAsync(f => f.Id == id && f.UploadedBy == userId && !f.IsDeleted);
            var file = files.FirstOrDefault();
            
            if (file == null)
                throw new ArgumentException("File not found.");

            // Update description
            file.Description = updateFileDto.Description;

            // Update file if a new one is provided
            if (updateFileDto.File != null && updateFileDto.File.Length > 0)
            {
                // Delete old file
                if (File.Exists(file.FilePath))
                {
                    File.Delete(file.FilePath);
                }

                // Save new file
                var fileName = $"{Guid.NewGuid()}_{updateFileDto.File.FileName}";
                var filePath = Path.Combine(_uploadDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updateFileDto.File.CopyToAsync(stream);
                }

                // Update file information
                file.FileName = updateFileDto.File.FileName;
                file.ContentType = updateFileDto.File.ContentType;
                file.FileSize = updateFileDto.File.Length;
                file.FilePath = filePath;
            }

            await _repository.UpdateAsync(file);
            return _mapper.Map<FileDto>(file);
        }

        public async Task DeleteFileAsync(int id, string userId)
        {
            var files = await _repository.FindAsync(f => f.Id == id && f.UploadedBy == userId && !f.IsDeleted);
            var file = files.FirstOrDefault();
            
            if (file == null)
                throw new ArgumentException("File not found.");

            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
            }

            file.IsDeleted = true;
            file.DeletedDate = DateTime.UtcNow;
            file.DeletedBy = userId;
            await _repository.UpdateAsync(file);
        }

        public async Task<(byte[] fileBytes, string fileName, string contentType)> DownloadFileAsync(int id, string userId)
        {
            var files = await _repository.FindAsync(f => f.Id == id && !f.IsDeleted);
            var file = files.FirstOrDefault();
            
            if (file == null)
                throw new ArgumentException("File not found.");

            if (!File.Exists(file.FilePath))
                throw new FileNotFoundException("File not found on disk.");

            var fileBytes = await File.ReadAllBytesAsync(file.FilePath);
            return (fileBytes, file.FileName, file.ContentType);
        }

        public async Task<long> GetUserTotalStorageAsync(string userId)
        {
            var files = await _repository.FindAsync(f => f.UploadedBy == userId && f.IsDeleted == false);
            return files.Sum(f => f.FileSize);
        }

        public async Task<(int fileCount, long totalSize)> GetUserStorageInfoAsync(string username)
        {
            var files = await _context.Files
                .Where(f => f.UploadedBy == username && !f.IsDeleted)
                .ToListAsync();

            return (files.Count, files.Sum(f => f.FileSize));
        }

        public async Task<IEnumerable<FileDto>> GetAllFilesForAdminAsync()
        {
            var files = await _context.Files
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            var fileDtos = _mapper.Map<IEnumerable<FileDto>>(files).ToList();
            
            // Format file sizes
            foreach (var fileDto in fileDtos)
            {
                fileDto.FormattedSize = FormatFileSize(fileDto.FileSize);
            }

            return fileDtos;
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
    }
} 