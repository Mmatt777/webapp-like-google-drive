using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order_Disc.Entities;
using Order_Disc.Models;
using System.IO.Compression;
using System.Security.Claims;

namespace Order_Disc.Controllers
{
    public class FolderController : Controller
    {
        private readonly AffDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FolderController(AffDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize]
        public IActionResult MyDisc()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserFolders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userFolders = await _context.Folders
                .Include(f => f.User)
                .Include(f => f.SharedWithUsers)
                .Where(f => (f.UserAccountId == userId || f.SharedWithUsers.Any(s => s.SharedWithUserId == userId))
                            && !f.IsImportant && !f.IsDeleted)
                .Select(f => new FolderVM
                {
                    Id = f.Id,
                    Name = f.Name,
                    CreatedDate = f.CreatedDate,
                    Path = f.Path,
                    OwnerName = f.UserAccountId == userId ? "Me" : (f.User != null ? $"{f.User.FirstName} {f.User.LastName}" : "Unknown")
                })
                .ToListAsync();

            ViewBag.UserId = userId;
            return View(userFolders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                TempData["Error"] = "Folder name cannot be empty.";
                return RedirectToAction("MyDisc", "Folder");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "User is not authenticated.";
                return RedirectToAction("Login", "Account");
            }

            var folderExists = await _context.Folders
                .AnyAsync(f => f.UserAccountId == userId && f.Name == folderName);

            if (folderExists)
            {
                TempData["Error"] = "A folder with this name already exists.";
                return RedirectToAction("MyDisc", "Folder");
            }

            var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders");
            var userFolderPath = Path.Combine(basePath, userId.ToString());
            var newFolderPath = Path.Combine(userFolderPath, folderName);

            try
            {
                if (!Directory.Exists(userFolderPath))
                {
                    Directory.CreateDirectory(userFolderPath);
                }

                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                }
                else
                {
                    TempData["Error"] = "Folder with this name already exists on the server.";
                    return RedirectToAction("MyDisc", "Folder");
                }

                var newFolder = new FolderEntity
                {
                    Name = folderName,
                    CreatedDate = DateTime.UtcNow,
                    UserAccountId = userId,
                    Path = newFolderPath,
                    OrginalFolderPath = newFolderPath
                };

                _context.Folders.Add(newFolder);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Error saving folder to database: {dbEx.Message}");
                    TempData["Error"] = "An error occurred while saving the folder to the database.";
                    return RedirectToAction("MyDisc", "Folder");
                }

                TempData["SuccessMessage"] = $"Folder '{folderName}' has been created successfully.";
                return RedirectToAction("MyDisc", "Folder");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder: {ex.Message}");
                TempData["Error"] = $"An error occurred while creating the folder: {ex.Message}";
                return RedirectToAction("MyDisc", "Folder");
            }
        }

        [HttpGet]
        [Route("Folder/Details/{folderId}")]
        public async Task<IActionResult> Details(int folderId)
        {
            try
            {
                Console.WriteLine($"Received folderId: {folderId}");

                if (folderId <= 0)
                {
                    Console.WriteLine("Invalid folder ID received.");
                    return BadRequest("Invalid folder ID.");
                }

                var folder = await _context.Folders
                    .Include(f => f.Files.Where(f => !f.IsDeleted && !f.IsImportant))
                    .FirstOrDefaultAsync(f => f.Id == folderId);

                if (folder == null)
                {
                    Console.WriteLine($"Folder with ID {folderId} not found.");
                    return NotFound("Folder not found.");
                }

                Console.WriteLine($"Folder found: {folder.Name}, with {folder.Files.Count} files.");
                Console.WriteLine($"Returning partial view 'FolderContents' with folder ID: {folder.Id}");

                return PartialView("FolderContents", folder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Details method: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("Download/folder/{folderId}")]
        public async Task<IActionResult> DownloadFolder(int folderId)
        {
            try
            {
                var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);
                if (folder == null)
                {
                    return NotFound("Folder not found.");
                }

                if (!Directory.Exists(folder.Path))
                {
                    return NotFound("Folder not found on the server.");
                }

                var tempZipPath = Path.Combine(Path.GetTempPath(), $"{folder.Name}.zip");

                if (System.IO.File.Exists(tempZipPath))
                {
                    System.IO.File.Delete(tempZipPath);
                }

                ZipFile.CreateFromDirectory(folder.Path, tempZipPath);

                var zipStream = new FileStream(tempZipPath, FileMode.Open, FileAccess.Read);
                return File(zipStream, "application/zip", $"{folder.Name}.zip");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving folder: {ex.Message}");
                return StatusCode(500, "A server error occurred.");
            }
        }
    }
}
