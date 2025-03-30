
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order_Disc.Entities;
using Order_Disc.Models;
using System.Security.Claims;

namespace Order_Disc.Controllers
{
    public class ImportantController : Controller
    {
        private readonly AffDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImportantController(AffDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpPost]
        [Route("Folder/MarkImportantFolder/{id}")]
        public async Task<IActionResult> MarkImportantFolder(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userBasePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
            var importantPath = Path.Combine(userBasePath, "Important");

            if (!Directory.Exists(importantPath))
            {
                Directory.CreateDirectory(importantPath);
            }

            var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == id && f.UserAccountId == userId);
            if (folder == null)
            {
                return NotFound("Folder not found.");
            }

            var folderName = folder.Name;
            var currentPath = folder.Path;
            var targetPath = Path.Combine(importantPath, folderName);

            try
            {
                if (!folder.IsImportant)
                {
                    if (string.IsNullOrEmpty(folder.OrginalFolderPath))
                    {
                        folder.OrginalFolderPath = currentPath;
                    }

                    if (!Directory.Exists(currentPath))
                    {
                        return NotFound("Folder does not exist in the file system.");
                    }

                    if (Directory.Exists(targetPath))
                    {
                        return Conflict("A folder with the same name already exists in Important.");
                    }

                    Directory.Move(currentPath, targetPath);
                    folder.Path = targetPath;
                }
                else
                {
                    var originalPath = folder.OrginalFolderPath;

                    if (string.IsNullOrEmpty(originalPath) || !Directory.Exists(targetPath))
                    {
                        return NotFound("Original folder path does not exist.");
                    }

                    if (Directory.Exists(originalPath))
                    {
                        return Conflict("A folder already exists at the original location.");
                    }

                    Directory.Move(targetPath, originalPath);
                    folder.Path = originalPath;
                    folder.OrginalFolderPath = null;
                }

                folder.IsImportant = !folder.IsImportant;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Folder importance status updated successfully." });
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, $"I/O Error: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        [Authorize]
        [Route("Folder/MarkImportantSharedFolder/{id}")]
        public async Task<IActionResult> MarkImportantSharedFolder(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userBasePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
            var importantPath = Path.Combine(userBasePath, "Important");
            var sharePath = Path.Combine(userBasePath, "Share");

            if (!Directory.Exists(importantPath))
            {
                Directory.CreateDirectory(importantPath);
            }

            var sharedFolderVM = await _context.FolderShares
                .Where(fs => fs.Id == id && fs.SharedWithUserId == userId)
                .Select(fs => new SharedFolderVM
                {
                    Id = fs.Id,
                    FolderId = fs.FolderId,
                    Name = fs.Folder.Name,
                    FolderShareId = fs.Id,
                    CreatedDate = fs.Folder.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (sharedFolderVM == null)
            {
                return NotFound("Shared folder not found.");
            }

            var folderName = sharedFolderVM.Name;

            var currentPath = Path.Combine(sharePath, folderName);
            var targetPath = Path.Combine(importantPath, folderName);

            try
            {
                var folderShare = await _context.FolderShares
                    .FirstOrDefaultAsync(fs => fs.Id == sharedFolderVM.FolderShareId);

                if (folderShare == null)
                {
                    return NotFound("Shared folder entry not found in database.");
                }

                if (!folderShare.IsImportant)
                {
                    if (!Directory.Exists(currentPath))
                    {
                        return NotFound("Shared folder does not exist in the file system.");
                    }

                    if (Directory.Exists(targetPath))
                    {
                        return Conflict("A folder with the same name already exists in Important.");
                    }

                    Directory.Move(currentPath, targetPath);

                    folderShare.OrginalFolderPath = currentPath; 
                    folderShare.ShareFolderPath = targetPath;
                }
                else
                { 
                    var originalPath = folderShare.OrginalFolderPath;

                    if (string.IsNullOrEmpty(originalPath))
                    {
                        return NotFound("Original path is missing.");
                    }

                    if (Directory.Exists(originalPath))
                    {
                        return Conflict("A folder already exists at the original location.");
                    }

                    Directory.Move(targetPath, originalPath);

                    folderShare.ShareFolderPath = originalPath;
                    folderShare.OrginalFolderPath = null;
                }

                folderShare.IsImportant = !folderShare.IsImportant;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Shared folder importance status updated successfully." });
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, $"I/O Error: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }





        [HttpPost]
        [Route("Important/MarkImportantFile/{id}")]
        public async Task<IActionResult> MarkImportantFile(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userBasePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
            var importantPath = Path.Combine(userBasePath, "Important");

            if (!Directory.Exists(importantPath))
            {
                Directory.CreateDirectory(importantPath);
            }

            var file = await _context.Files
                .Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.Id == id && f.Folder.UserAccountId == userId);

            if (file == null)
            {
                return NotFound("File not found.");
            }

            var fileName = file.FileName;

            var currentPath = string.IsNullOrEmpty(file.OrginalFilePath)
                ? file.FilePath
                : file.OrginalFilePath;

            var targetPath = Path.Combine(importantPath, fileName);

            try
            {
                if (!file.IsImportant)
                {
                    if (string.IsNullOrEmpty(file.OrginalFilePath))
                    {
                        file.OrginalFilePath = currentPath;
                    }

                    if (!System.IO.File.Exists(currentPath))
                    {
                        return NotFound("File does not exist in the file system.");
                    }

                    if (System.IO.File.Exists(targetPath))
                    {
                        return Conflict("A file with the same name already exists in Important.");
                    }

                    System.IO.File.Move(currentPath, targetPath);
                    file.FilePath = targetPath;
                }
                else
                {
                    if (string.IsNullOrEmpty(file.OrginalFilePath) || !System.IO.File.Exists(targetPath))
                    {
                        return NotFound("File in Important does not exist or original path is missing.");
                    }

                    var originalPath = file.OrginalFilePath;

                    if (System.IO.File.Exists(originalPath))
                    {
                        return Conflict("A file already exists at the original location.");
                    }

                    System.IO.File.Move(targetPath, originalPath);
                    file.FilePath = originalPath;

                    file.OrginalFilePath = null;
                }

                file.IsImportant = !file.IsImportant;
                await _context.SaveChangesAsync();

                return Ok(new { message = "File importance status updated successfully." });
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, $"I/O Error: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
        [HttpPost]
        [Route("Important/MarkImportantSharedFile/{id}")]
        public async Task<IActionResult> MarkImportantSharedFile(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userBasePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
            var importantPath = Path.Combine(userBasePath, "Important");
            var sharePath = Path.Combine(userBasePath, "Share");

            if (!Directory.Exists(importantPath))
            {
                Directory.CreateDirectory(importantPath);
            }

            var sharedFile = await _context.FileShares
                .Include(fs => fs.File)
                .FirstOrDefaultAsync(fs => fs.FileId == id && fs.SharedWithUserId == userId);

            if (sharedFile == null)
            {
                return NotFound("Shared file not found.");
            }

            var file = sharedFile.File;
            var fileName = file.FileName;

            var currentPath = string.IsNullOrEmpty(sharedFile.OrginalFilePath)
                ? Path.Combine(sharePath, fileName)
                : sharedFile.OrginalFilePath;

            var targetPath = Path.Combine(importantPath, fileName);

            try
            {
                if (!sharedFile.IsImportant)
                {
                    if (string.IsNullOrEmpty(sharedFile.OrginalFilePath))
                    {
                        sharedFile.OrginalFilePath = currentPath;
                    }

                    if (!System.IO.File.Exists(currentPath))
                    {
                        return NotFound("Shared file does not exist in the file system.");
                    }

                    if (System.IO.File.Exists(targetPath))
                    {
                        return Conflict("A file with the same name already exists in Important.");
                    }

                    System.IO.File.Move(currentPath, targetPath);
                    file.FilePath = targetPath; 
                }
                else
                {
                    if (string.IsNullOrEmpty(sharedFile.OrginalFilePath) || !System.IO.File.Exists(targetPath))
                    {
                        return NotFound("File in Important does not exist or original path is missing.");
                    }

                    var originalPath = sharedFile.OrginalFilePath;

                    if (System.IO.File.Exists(originalPath))
                    {
                        return Conflict("A file already exists at the original location.");
                    }

                    System.IO.File.Move(targetPath, originalPath);
                    file.FilePath = originalPath;

                    sharedFile.OrginalFilePath = null;
                }

                sharedFile.IsImportant = !sharedFile.IsImportant;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Shared file importance status updated successfully." });
            }
            catch (IOException ioEx)
            {
                return StatusCode(500, $"I/O Error: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("Folder/AllImportantItems")]
        public async Task<IActionResult> AllImportantItems()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            
            var ownImportantFolders = await _context.Folders
                .Where(f => f.UserAccountId == userId && f.IsImportant && !f.IsDeleted)
                .Select(f => new ImportantVM
                {
                    Id = f.Id,
                    Name = f.Name,
                    Path = f.Path,
                    Type = "folder",
                    OwnerName = "Me",
                    CreatedDate = f.CreatedDate
                })
                .ToListAsync();

            
            var sharedImportantFolders = await _context.FolderShares
                .Where(fs => fs.SharedWithUserId == userId && fs.IsImportant && !fs.IsDeleted)
                .Select(fs => new ImportantVM
                {
                    Id = fs.Id,
                    Name = fs.Folder.Name,
                    Path = fs.ShareFolderPath,
                    Type = "shared-folder",
                    OwnerName = $"{fs.SharedByUser.FirstName} {fs.SharedByUser.LastName}",
                    CreatedDate = fs.Folder.CreatedDate
                })
                .ToListAsync();

            
            var ownImportantFiles = await _context.Files
                .Where(f => f.Folder.UserAccountId == userId && f.IsImportant && !f.IsDeleted)
                .Select(f => new ImportantVM
                {
                    Id = f.Id,
                    Name = f.FileName,
                    Path = f.FilePath,
                    Type = "file",
                    OwnerName = "Me",
                    CreatedDate = f.UploadDate
                })
                .ToListAsync();

            
            var sharedImportantFiles = await _context.FileShares
                .Where(fs => fs.SharedWithUserId == userId && fs.IsImportant && !fs.IsDeleted)
                .Select(fs => new ImportantVM
                {
                    Id = fs.FileId,
                    folderShareId = fs.Id,
                    Name = fs.File.FileName,
                    Path = fs.ShareFilePath,
                    Type = "shared-file",
                    OwnerName = $"{fs.SharedByUser.FirstName} {fs.SharedByUser.LastName}",
                    CreatedDate = fs.File.UploadDate
                })
                .ToListAsync();

           
            var allImportantItems = ownImportantFolders
                .Concat(sharedImportantFolders)
                .Concat(ownImportantFiles)
                .Concat(sharedImportantFiles)
                .ToList();

            return PartialView("~/Views/Folder/AllImportantItems.cshtml", allImportantItems);
        }
        [HttpPost]
        [Route("Folder/UnmarkImportant/{id}")]
        public async Task<IActionResult> UnmarkImportant(int id, [FromQuery] string type)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            if (type == "folder")
            {
                var folder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == id && f.UserAccountId == userId && f.IsImportant);

                if (folder == null)
                {
                    return NotFound("Folder not found or not marked as Important.");
                }

                if (string.IsNullOrWhiteSpace(folder.OrginalFolderPath))
                {
                    return BadRequest("Original folder path is not set.");
                }

                var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
                var importantPath = Path.Combine(basePath, "Important", folder.Name);

                try
                {
                    if (!Directory.Exists(importantPath))
                    {
                        return NotFound("Important folder path does not exist.");
                    }

                    if (folder.Path == folder.OrginalFolderPath)
                    {
                        var newPath = Path.Combine(basePath, folder.Name);
                        Directory.Move(folder.Path, newPath);
                        folder.Path = newPath;
                        folder.OrginalFolderPath = "";
                    }
                    else
                    {
                        Directory.Move(importantPath, folder.OrginalFolderPath);
                        folder.Path = folder.OrginalFolderPath;
                    }

                    folder.IsImportant = false;

                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Folder restored successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while restoring the folder: {ex.Message}");
                }
            }

            if (type == "shared-folder")
            {
                var sharedFolder = await _context.FolderShares
                    .Include(fs => fs.Folder)
                    .FirstOrDefaultAsync(fs => fs.Id == id && fs.SharedWithUserId == userId && fs.IsImportant);

                if (sharedFolder == null)
                {
                    return NotFound("Shared folder not found or not marked as Important.");
                }

                if (string.IsNullOrWhiteSpace(sharedFolder.OrginalFolderPath))
                {
                    return BadRequest("Original folder path is not set.");
                }

                var sharePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", sharedFolder.SharedWithUserId.ToString(), "Share", sharedFolder.Folder.Name);
                var importantPath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", sharedFolder.SharedWithUserId.ToString(), "Important", sharedFolder.Folder.Name);

                try
                {
                    if (!Directory.Exists(importantPath))
                    {
                        return NotFound($"Important folder path does not exist: {importantPath}");
                    }

                    if (Directory.Exists(sharePath))
                    {
                        return Conflict($"Folder already exists in the Share path: {sharePath}");
                    }

                    Directory.Move(importantPath, sharePath);

                    sharedFolder.ShareFolderPath = sharePath;

                    if (sharedFolder.ShareFolderPath == sharedFolder.OrginalFolderPath)
                    {
                        sharedFolder.OrginalFolderPath = "";
                    }

                    sharedFolder.IsImportant = false;

                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Folder successfully moved back to Share." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while moving the folder: {ex.Message}");
                }
            }       
            return BadRequest("Invalid item type.");
        }
    }
}
