using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Resolves the physical upload root directory.
        /// Uses "Storage:UploadRoot" when configured; otherwise falls back to "{WebRootPath}/uploads".
        /// </summary>
        string GetUploadRootPath();

        /// <summary>
        /// Resolves and ensures the physical system upload directory ("{UploadRoot}/system").
        /// </summary>
        string GetSystemUploadDirectory();

        /// <summary>
        /// Saves an uploaded system logo with a GUID-based safe filename and returns its public relative URL path ("/uploads/system/{filename}").
        /// </summary>
        Task<string> SaveSystemLogoAsync(IFormFile file, string extension);

        /// <summary>
        /// Resolves a stored relative public URL path (e.g. "/uploads/system/agam-logo-xxx.png")
        /// to its verified physical path inside the managed system upload directory, preventing path traversal.
        /// </summary>
        bool TryResolveManagedSystemFilePath(string? relativeUrlPath, out string physicalPath);

        /// <summary>
        /// Safely deletes a managed system upload file if it resides inside the configured system upload directory.
        /// Never deletes "/images/logo.png" or any file outside "/uploads/system/".
        /// </summary>
        bool TryDeleteManagedSystemFile(string? relativeUrlPath);

        /// <summary>
        /// Resolves and ensures the physical blog upload directory ("{UploadRoot}/blog").
        /// </summary>
        string GetBlogUploadDirectory();

        /// <summary>
        /// Saves an uploaded blog featured image with a GUID-based safe filename ("blog-{guid}{ext}")
        /// and returns its public relative URL path ("/uploads/blog/{filename}").
        /// </summary>
        Task<string> SaveBlogImageAsync(IFormFile file, string extension);

        /// <summary>
        /// Resolves a stored relative public URL path (e.g. "/uploads/blog/blog-xxx.jpg")
        /// to its verified physical path inside the managed blog upload directory, preventing path traversal.
        /// </summary>
        bool TryResolveManagedBlogFilePath(string? relativeUrlPath, out string physicalPath);

        /// <summary>
        /// Safely deletes a managed blog image file if it resides strictly inside the configured blog upload directory ("/uploads/blog/").
        /// </summary>
        bool TryDeleteManagedBlogFile(string? relativeUrlPath);
    }
}
