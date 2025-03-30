using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Order_Disc.Entities;
using Order_Disc.Models;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Order_Disc.Controllers
{
    public class FilesController : Controller
    {
        private readonly AffDbContext _context;

        public FilesController(AffDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("Folder/UploadFiles/{folderId}")]
        public async Task<IActionResult> UploadFiles(int folderId, List<IFormFile> files)
        {
            try
            {
                Console.WriteLine($"Uploading files to folder ID: {folderId}");
                Console.WriteLine($"Number of files received: {files?.Count ?? 0}");

                if (files == null || !files.Any())
                {
                    return BadRequest("No files received.");
                }

                var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);
                if (folder == null)
                {
                    return NotFound("Folder not found.");
                }

                var folderPath = folder.Path;
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Folder path does not exist. Creating: {folderPath}");
                    Directory.CreateDirectory(folderPath);
                }

                foreach (var file in files)
                {
                    Console.WriteLine($"Processing file: {file.FileName}");

                    var filePath = Path.Combine(folderPath, file.FileName);
                    var fileType = Path.GetExtension(file.FileName)?.ToLower() ?? "unknown";

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var newFile = new FileEntity
                    {
                        FileName = file.FileName,
                        FilePath = filePath,
                        OrginalFilePath = filePath,
                        FolderId = folderId,
                        SizeInBytes = file.Length,
                        UploadDate = DateTime.UtcNow,
                        FileType = fileType,
                        IsDeleted = false,
                        IsImportant = false
                    };
                    _context.Files.Add(newFile);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Files uploaded successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading files: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("File/MoveFileToTrash/{fileId}")]
        public async Task<IActionResult> MoveFileToTrash(int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
                return NotFound("File not found.");
            }

            file.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "File moved to trash." });
        }

        [HttpGet]
        [Route("Download/file/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId, bool shared = false)
        {

            var file = await _context.Files
                .Include(f => f.SharedFiles)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return NotFound("File not found.");
            }

            if (shared)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                {
                    return Unauthorized("User is not authenticated.");
                }

                if (!file.SharedFiles.Any(s => s.SharedWithUserId == currentUserId))
                {
                    return Forbid("Access to the shared file is denied.");
                }
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                return NotFound("File not found on the server.");
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.FilePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileStream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType, file.FileName);
        }


    }
}
