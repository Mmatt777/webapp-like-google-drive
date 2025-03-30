using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order_Disc.Entities;
using Order_Disc.Models;
using System.IO.Compression;
using System.Security.Claims;

namespace Order_Disc.Controllers
{
    public class ShareController : Controller
    {
        private readonly AffDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShareController(AffDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        [Route("Folder/ShareFolder")]
        public async Task<IActionResult> ShareFolder([FromBody] SharedFolderVM request, [FromQuery] bool overwrite = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int sharedByUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userToShareWith = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (userToShareWith == null)
            {
                return BadRequest("User with the provided email address not found.");
            }

            var folder = await _context.Folders
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.Id == request.FolderId);

            if (folder == null || string.IsNullOrEmpty(folder.Name))
            {
                return NotFound("Folder not found or folder has invalid data.");
            }

            _context.Entry(folder).State = EntityState.Unchanged;

            var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userToShareWith.UserId.ToString());
            var shareFolderPath = Path.Combine(basePath, "Share");
            var sharedFolderPath = Path.Combine(shareFolderPath, folder.Name);

            var existingFolderShare = await _context.FolderShares.FirstOrDefaultAsync(fs =>
                fs.FolderId == folder.Id && fs.SharedWithUserId == userToShareWith.UserId);

            try
            {
                if (!Directory.Exists(shareFolderPath))
                {
                    Directory.CreateDirectory(shareFolderPath);
                }

                if (overwrite && Directory.Exists(sharedFolderPath))
                {
                    Directory.Delete(sharedFolderPath, true);

                    var fileSharesToDelete = _context.FileShares
                        .Where(fs => fs.SharedWithUserId == userToShareWith.UserId && folder.Files.Any(f => f.Id == fs.FileId));

                    _context.FileShares.RemoveRange(fileSharesToDelete);
                }

                CopyFolderAndUpdateDatabase(folder, sharedFolderPath, userToShareWith.UserId, sharedByUserId);

                if (existingFolderShare == null)
                {
                    existingFolderShare = new FolderShare
                    {
                        FolderId = folder.Id,
                        SharedWithUserId = userToShareWith.UserId,
                        SharedByUserId = sharedByUserId,
                        AccessLevel = "read",
                        ShareFolderPath = sharedFolderPath,
                        OrginalFolderPath = folder.Path,
                        IsDeleted = false
                    };
                    _context.FolderShares.Add(existingFolderShare);
                }
                else
                {
                    existingFolderShare.ShareFolderPath = sharedFolderPath;

                    if (string.IsNullOrEmpty(existingFolderShare.OrginalFolderPath))
                    {
                        existingFolderShare.OrginalFolderPath = folder.Path;
                    }

                    _context.FolderShares.Update(existingFolderShare);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error during folder copy: {ex.Message}");
            }

            return Ok(new { message = "Folder and its files have been successfully shared." });
        }

        private void CopyFolderAndUpdateDatabase(FolderEntity folder, string destinationPath, int sharedWithUserId, int sharedByUserId)
        {
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (var file in folder.Files)
            {
                var sourceFilePath = Path.GetFullPath(file.FilePath.Trim());
                if (!System.IO.File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Source file does not exist: {sourceFilePath}");
                }

                var targetFilePath = Path.Combine(destinationPath, file.FileName);
                System.IO.File.Copy(sourceFilePath, targetFilePath, true);

                var newFileShare = new FileShareEntity
                {
                    FileId = file.Id,
                    SharedWithUserId = sharedWithUserId,
                    SharedByUserId = sharedByUserId,
                    ShareFilePath = targetFilePath
                };
                _context.FileShares.Add(newFileShare);
            }
        }

        [HttpGet]
        [Route("Folder/SharedFolders")]
        [Authorize]
        public async Task<IActionResult> SharedFolders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var sharedFolders = await _context.FolderShares
                .Include(fs => fs.Folder)
                .Include(fs => fs.SharedByUser)
                .Where(fs => fs.SharedWithUserId == userId && !fs.IsDeleted && !fs.IsImportant)
                .ToListAsync();

            var sharedFolderVMs = sharedFolders.Select(fs => new SharedFolderVM
            {
                FolderShareId = fs.Id,
                Id = fs.FolderId,
                Name = fs.Folder.Name,
                SharedByFirstName = fs.SharedByUser.FirstName,
                SharedByLastName = fs.SharedByUser.LastName,
                CreatedDate = fs.Folder.CreatedDate
            }).ToList();

            return View("~/Views/Folder/SharedFolders.cshtml", sharedFolderVMs);
        }

        [HttpGet]
        [Route("Folder/SharedFiles")]
        [Authorize]
        public async Task<IActionResult> SharedFiles(int folderId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("User not authenticated.");
                }

                var folderShare = await _context.FolderShares
                    .Where(fs => fs.Id == folderId && fs.SharedWithUserId == userId)
                    .Select(fs => fs.FolderId)
                    .FirstOrDefaultAsync();

                if (folderShare == 0)
                {
                    return NotFound("Shared folder not found or permission denied.");
                }

                var sharedFiles = await _context.FileShares
                    .Include(fs => fs.File)
                    .Include(fs => fs.SharedByUser)
                    .Where(fs => fs.SharedWithUserId == userId
                                 && fs.File.FolderId == folderShare
                                 && !fs.IsDeleted
                                 && !fs.File.IsDeleted)
                    .ToListAsync();

                if (!sharedFiles.Any())
                {
                    return PartialView("~/Views/Folder/SharedFiles.cshtml", new List<FileShareVM>());
                }

                var sharedFileVMs = sharedFiles.Select(fs => new FileShareVM
                {
                    FileShareId = fs.Id,
                    Id = fs.File.Id,
                    FileName = fs.File.FileName,
                    FilePath = fs.File.FilePath,
                    SizeInBytes = fs.File.SizeInBytes,
                    UploadDate = fs.File.UploadDate,
                    SharedByFirstName = fs.SharedByUser?.FirstName ?? "Unknown",
                    SharedByLastName = fs.SharedByUser?.LastName ?? "User"
                }).ToList();

                return PartialView("~/Views/Folder/SharedFiles.cshtml", sharedFileVMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while loading shared files.");
            }
        }
    }
}
