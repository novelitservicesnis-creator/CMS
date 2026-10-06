using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const string ManagedSystemUrlPrefix = "/uploads/system/";
        private const string DefaultFallbackLogoPath = "/images/logo.png";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILogger<FileStorageService> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
        }

        public string GetUploadRootPath()
        {
            var configuredRoot = _configuration["Storage:UploadRoot"];
            string uploadRoot;

            if (string.IsNullOrWhiteSpace(configuredRoot))
            {
                var webRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath)
                    ? _env.WebRootPath
                    : Path.Combine(_env.ContentRootPath, "wwwroot");
                uploadRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
            }
            else
            {
                uploadRoot = Path.GetFullPath(configuredRoot.Trim());
            }

            if (!Directory.Exists(uploadRoot))
            {
                Directory.CreateDirectory(uploadRoot);
            }

            return uploadRoot;
        }

        public string GetSystemUploadDirectory()
        {
            var uploadRoot = GetUploadRootPath();
            var systemUploadDirectory = Path.GetFullPath(Path.Combine(uploadRoot, "system"));

            if (!Directory.Exists(systemUploadDirectory))
            {
                Directory.CreateDirectory(systemUploadDirectory);
            }

            return systemUploadDirectory;
        }

        public async Task<string> SaveSystemLogoAsync(IFormFile file, string extension)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Uploaded file cannot be empty.", nameof(file));
            }

            var ext = (extension ?? string.Empty).Trim().ToLowerInvariant();
            if (!ext.StartsWith("."))
            {
                ext = "." + ext;
            }

            var safeFileName = $"agam-logo-{Guid.NewGuid():N}{ext}";
            var systemUploadDirectory = GetSystemUploadDirectory();

            var physicalPath = Path.GetFullPath(Path.Combine(systemUploadDirectory, safeFileName));
            var normalizedDirWithSep = systemUploadDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                       + Path.DirectorySeparatorChar;

            if (!physicalPath.StartsWith(normalizedDirWithSep, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Resolved upload path is outside the managed system upload directory.");
            }

            using (var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            return $"{ManagedSystemUrlPrefix}{safeFileName}";
        }

        public bool TryResolveManagedSystemFilePath(string? relativeUrlPath, out string physicalPath)
        {
            physicalPath = string.Empty;

            if (string.IsNullOrWhiteSpace(relativeUrlPath))
            {
                return false;
            }

            var normalizedUrl = relativeUrlPath.Trim().Replace('\\', '/');

            // Never resolve the default fallback logo or anything outside /uploads/system/
            if (string.Equals(normalizedUrl, DefaultFallbackLogoPath, StringComparison.OrdinalIgnoreCase) ||
                !normalizedUrl.StartsWith(ManagedSystemUrlPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Reject traversal tokens in relative URL
            if (normalizedUrl.Contains("..", StringComparison.Ordinal))
            {
                return false;
            }

            var fileName = normalizedUrl.Substring(ManagedSystemUrlPrefix.Length).Trim();
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName.Contains('/') ||
                fileName.Contains('\\') ||
                fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                var uploadRoot = GetUploadRootPath();
                var systemUploadDirectory = Path.GetFullPath(Path.Combine(uploadRoot, "system"));
                var normalizedRootWithSep = uploadRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                            + Path.DirectorySeparatorChar;
                var normalizedSystemWithSep = systemUploadDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                              + Path.DirectorySeparatorChar;

                var candidatePath = Path.GetFullPath(Path.Combine(systemUploadDirectory, fileName));

                // Verify candidate path is strictly inside both uploadRoot and systemUploadDirectory
                if (!candidatePath.StartsWith(normalizedRootWithSep, StringComparison.OrdinalIgnoreCase) ||
                    !candidatePath.StartsWith(normalizedSystemWithSep, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                physicalPath = candidatePath;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve managed file path for '{RelativeUrlPath}'.", relativeUrlPath);
                return false;
            }
        }

        public bool TryDeleteManagedSystemFile(string? relativeUrlPath)
        {
            if (!TryResolveManagedSystemFilePath(relativeUrlPath, out var physicalPath))
            {
                return false;
            }

            try
            {
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to delete old managed system file at '{PhysicalPath}'.", physicalPath);
                return false;
            }
        }
    }
}
