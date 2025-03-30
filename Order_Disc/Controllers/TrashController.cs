using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order_Disc.Entities;
using Order_Disc.Models;
using System.IO;
using System.Security.Claims;

public class TrashController : Controller
{
    private readonly AffDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TrashController(AffDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> MoveToTrash(int id, int? folderId = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("User not authenticated.");
        }

        var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
        var trashFolderPath = Path.Combine(basePath, "Trash");

        if (!Directory.Exists(trashFolderPath))
        {
            Directory.CreateDirectory(trashFolderPath);
        }

        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.Folder.UserAccountId == userId);
        if (file == null)
        {
            return NotFound("File not found.");
        }

        var currentPath = file.FilePath;
        var newPath = Path.Combine(trashFolderPath, file.FileName);

        if (!System.IO.File.Exists(currentPath))
        {
            return NotFound("File path does not exist in the file system.");
        }

        file.OrginalFilePath = currentPath;

        System.IO.File.Move(currentPath, newPath);

        file.FilePath = newPath;
        file.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "File moved to Trash.", folderId = folderId });
    }

    [HttpPost]
    [Route("Folder/MoveFolderToTrash/{folderId}")]
    public async Task<IActionResult> MoveFolderToTrash(int folderId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("User not authenticated.");
        }

        Console.WriteLine($"Attempting to move folder with ID: {folderId}, by user with ID: {userId}");

        var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
        var trashFolderPath = Path.Combine(basePath, "Trash");

        if (!Directory.Exists(trashFolderPath))
        {
            Directory.CreateDirectory(trashFolderPath);
        }

        var folderVM = await _context.Folders
            .Where(f => f.Id == folderId && f.UserAccountId == userId)
            .Select(f => new FolderVM
            {
                Id = f.Id,
                Name = f.Name,
                Path = f.Path,
                OrginalFolderPath = f.Path,
                IsDeleted = f.IsDeleted
            })
            .FirstOrDefaultAsync();

        if (folderVM == null)
        {
            Console.WriteLine("Folder not found in the database.");
            return NotFound("Folder not found or permission denied.");
        }

        if (folderVM.IsDeleted)
        {
            return BadRequest("The folder has already been moved to trash.");
        }

        var currentPath = folderVM.Path;
        if (string.IsNullOrEmpty(currentPath) || !Directory.Exists(currentPath))
        {
            Console.WriteLine($"Folder path does not exist: {currentPath}");
            return NotFound("Folder path does not exist.");
        }

        var trashPath = Path.Combine(trashFolderPath, folderVM.Name);

        try
        {
            Directory.Move(currentPath, trashPath);

            var folderToUpdate = await _context.Folders.FirstOrDefaultAsync(f => f.Id == folderVM.Id);
            folderToUpdate.Path = trashPath;
            folderToUpdate.IsDeleted = true;
            folderToUpdate.OrginalFolderPath = folderVM.OrginalFolderPath;

            await _context.SaveChangesAsync();

            Console.WriteLine($"Folder moved to trash: {trashPath}");
            return Ok(new { message = "Folder has been moved to trash." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving folder: {ex.Message}");
            return StatusCode(500, $"Error moving folder: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("Trash/GetTrashContents")]
    public async Task<IActionResult> GetTrashContents()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("User not authenticated.");
        }

        var deletedFolders = await _context.Folders
            .Where(f => f.UserAccountId == userId && f.IsDeleted)
            .Select(f => new FolderVM
            {
                Id = f.Id,
                Name = f.Name,
                CreatedDate = f.CreatedDate,
                IsShared = false
            })
            .ToListAsync();

        var deletedSharedFolders = await _context.FolderShares
            .Include(fs => fs.Folder)
            .Where(fs => fs.SharedWithUserId == userId && fs.IsDeleted)
            .Select(fs => new FolderVM
            {
                Id = fs.Folder.Id,
                FolderShareId = fs.Id,
                Name = fs.Folder.Name,
                CreatedDate = fs.Folder.CreatedDate,
                IsShared = true
            })
            .ToListAsync();


        var deletedFiles = await _context.Files
            .Where(f => f.Folder.UserAccountId == userId && f.IsDeleted)
            .Select(f => new FilesVM
            {
                Id = f.Id,
                FileName = f.FileName,
                SizeInBytes = f.SizeInBytes,
                UploadDate = f.UploadDate,
                IsShared = false
            })
            .ToListAsync();

        var deletedSharedFiles = await _context.FileShares
            .Include(fs => fs.File)
            .Where(fs => fs.SharedWithUserId == userId && fs.IsDeleted)
            .Select(fs => new FilesVM
            {
                Id = fs.File.Id,
                FileShareId = fs.Id,
                FileName = fs.File.FileName,
                FilePath = fs.File.FilePath,
                SizeInBytes = fs.File.SizeInBytes,
                UploadDate = fs.File.UploadDate,
                IsShared = true
            })
            .ToListAsync();


        return PartialView("~/Views/Folder/TrashContents.cshtml", new TrashVM
        {
            SubFolders = deletedFolders.Concat(deletedSharedFolders).ToList(),
            Files = deletedFiles.Concat(deletedSharedFiles).ToList()
        });
    }



    [HttpPost]
    [Route("Trash/Restore/{id}")]
    public async Task<IActionResult> Restore(int id, [FromQuery] string type)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("User not authenticated.");
        }

        var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());

        if (type == "folder")
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted);
            if (folder == null) return NotFound("Folder not found in Trash.");

            return await RestoreFolder(folder, basePath);
        }
        else if (type == "shared-folder")
        {
            var sharedFolder = await _context.FolderShares
                .Include(fs => fs.Folder)
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.IsDeleted);

            if (sharedFolder == null)
                return NotFound("Shared folder not found in Trash.");

            return await RestoreSharedFolder(sharedFolder, basePath);
        }

        else if (type == "file")
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted);
            if (file == null) return NotFound("File not found in Trash.");

            return await RestoreFile(file, basePath);
        }

        else if (type == "shared-file")
        {
            var sharedFile = await _context.FileShares
                .Include(fs => fs.File)
                .FirstOrDefaultAsync(fs => fs.Id == id && fs.SharedWithUserId == userId && fs.IsDeleted);
            if (sharedFile == null) return NotFound("Shared file not found in Trash.");

            return await RestoreSharedFile(sharedFile, basePath);
        }

        return BadRequest("Invalid item type.");
    }


    private async Task<IActionResult> RestoreFolder(FolderEntity folder, string basePath)
    {

        var trashPath = Path.Combine(basePath, "Trash", folder.Name);


        var originalPath = folder.OrginalFolderPath;

        if (string.IsNullOrWhiteSpace(originalPath))
            return BadRequest("Original path is not set for this folder.");

        if (!Directory.Exists(trashPath))
            return NotFound("Folder does not exist in Trash.");

        if (Directory.Exists(originalPath))
            return Conflict("A folder with the same name already exists at the original location.");

        try
        {

            Directory.Move(trashPath, originalPath);


            folder.IsDeleted = false;
            folder.Path = originalPath;


            await _context.SaveChangesAsync();

            return Ok(new { message = "Folder restored successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while restoring the folder: {ex.Message}");
        }
    }



    private async Task<IActionResult> RestoreSharedFolder(FolderShare sharedFolder, string basePath)
    {
        var trashPath = sharedFolder.ShareFolderPath;
        var restorePath = Path.Combine(basePath, "Share", sharedFolder.Folder.Name);

        if (!Directory.Exists(trashPath))
            return NotFound("Shared folder does not exist in Trash.");

        if (Directory.Exists(restorePath))
            return Conflict("A folder with the same name already exists.");

        Directory.Move(trashPath, restorePath);
        sharedFolder.IsDeleted = false;
        sharedFolder.IsImportant = false;
        sharedFolder.ShareFolderPath = restorePath;
        sharedFolder.OrginalFolderPath = basePath;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Shared folder restored successfully." });
    }


    private async Task<IActionResult> RestoreFile(FileEntity file, string basePath)
    {
        var trashPath = file.FilePath;
        var restorePath = file.OrginalFilePath;

        if (!System.IO.File.Exists(trashPath))
            return NotFound("File does not exist in Trash.");

        if (System.IO.File.Exists(restorePath))
            return Conflict("A file with the same name already exists.");

        System.IO.File.Move(trashPath, restorePath);
        file.IsDeleted = false;
        file.FilePath = restorePath;
        file.OrginalFilePath = basePath;

        await _context.SaveChangesAsync();
        return Ok(new { message = "File restored successfully." });
    }



    private async Task<IActionResult> RestoreSharedFile(FileShareEntity sharedFile, string basePath)
    {
        var trashPath = sharedFile.ShareFilePath;
        var restorePath = sharedFile.OrginalFilePath; 

        if (!System.IO.File.Exists(trashPath))
            return NotFound("Shared file does not exist in Trash.");

        if (System.IO.File.Exists(restorePath))
            return Conflict("A file with the same name already exists.");

        System.IO.File.Move(trashPath, restorePath);
        sharedFile.IsDeleted = false;
        sharedFile.ShareFilePath = restorePath;
        sharedFile.OrginalFilePath = basePath;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Shared file restored successfully." });
    }


    [HttpPost]
    [Route("Trash/MoveSharedFolderToTrash/{folderShareId}")]
    public async Task<IActionResult> MoveSharedFolderToTrash(int folderShareId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            Console.WriteLine("Unauthorized access attempt.");
            return Unauthorized("User not authenticated.");
        }

        Console.WriteLine($"Received folderShareId: {folderShareId}, UserId: {userId}");

        var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
        var trashFolderPath = Path.Combine(basePath, "Trash");

        if (!Directory.Exists(trashFolderPath))
        {
            Directory.CreateDirectory(trashFolderPath);
        }

        var sharedFolderVM = await _context.FolderShares
            .Where(fs => fs.Id == folderShareId && fs.SharedWithUserId == userId)
            .Select(fs => new SharedFolderVM
            {
                FolderShareId = fs.Id,
                Name = fs.Folder.Name,
                FolderId = fs.FolderId,
                CreatedDate = fs.Folder.CreatedDate,
                SharedByFirstName = fs.SharedByUser.FirstName,
                SharedByLastName = fs.SharedByUser.LastName,
            })
            .FirstOrDefaultAsync();

        if (sharedFolderVM == null)
        {
            Console.WriteLine("Shared folder not found in database.");
            return NotFound("Shared folder not found.");
        }

        var sharedFolder = await _context.FolderShares
            .Include(fs => fs.Folder)
            .FirstOrDefaultAsync(fs => fs.Id == folderShareId);

        if (sharedFolder == null || string.IsNullOrEmpty(sharedFolder.ShareFolderPath))
        {
            Console.WriteLine($"Shared folder path does not exist or is null.");
            return NotFound("Shared folder path does not exist.");
        }

        var currentPath = sharedFolder.ShareFolderPath;
        var trashPath = Path.Combine(trashFolderPath, sharedFolderVM.Name);

        if (!Directory.Exists(currentPath))
        {
            Console.WriteLine($"Shared folder path does not exist: {currentPath}");
            return NotFound("Shared folder path does not exist.");
        }

        try
        {
            Directory.Move(currentPath, trashPath);

            sharedFolder.IsDeleted = true;
            sharedFolder.ShareFolderPath = trashPath;

            await _context.SaveChangesAsync();

            Console.WriteLine($"Shared folder moved to Trash: {trashPath}");
            return Ok(new { message = "Shared folder moved to Trash." });
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"I/O error occurred: {ioEx.Message}");
            return StatusCode(500, $"Error moving shared folder: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving shared folder: {ex.Message}");
            return StatusCode(500, $"Unexpected error: {ex.Message}");
        }
    }





    [HttpPost]
    [Route("Trash/MoveSharedFilesToTrash/{fileShareId}")]
    public async Task<IActionResult> MoveSharedFilesToTrash(int fileShareId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("User not authenticated.");
        }

        var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
        var trashFolderPath = Path.Combine(basePath, "Trash");

        if (!Directory.Exists(trashFolderPath))
        {
            Directory.CreateDirectory(trashFolderPath);
        }

        var sharedFile = await _context.FileShares
            .Include(fs => fs.File)
            .FirstOrDefaultAsync(fs => fs.Id == fileShareId && fs.SharedWithUserId == userId);

        if (sharedFile == null)
        {
            return NotFound("Shared file not found.");
        }

        var sharedFilePath = sharedFile.ShareFilePath;
        var newTrashPath = Path.Combine(trashFolderPath, sharedFile.File.FileName);

        if (!System.IO.File.Exists(sharedFilePath))
        {
            return NotFound("Shared file path does not exist.");
        }

        sharedFile.OrginalFilePath = sharedFile.ShareFilePath;

        System.IO.File.Move(sharedFilePath, newTrashPath);

        sharedFile.IsDeleted = true;
        sharedFile.ShareFilePath = newTrashPath;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Shared file moved to Trash." });
    }


    [HttpPost]
    [Route("Trash/DeleteAll")]
    public async Task<IActionResult> DeleteAllTrash()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("User not authenticated.");
        }

        var trashBasePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString(), "Trash");

        var deletedFolders = await _context.Folders
            .Include(f => f.Files)
            .Where(f => f.UserAccountId == userId && f.IsDeleted)
            .ToListAsync();

        var deletedFiles = await _context.Files
            .Where(f => f.Folder.UserAccountId == userId && f.IsDeleted)
            .ToListAsync();

        var deletedSharedFolders = await _context.FolderShares
            .Include(fs => fs.Folder)
            .Where(fs => fs.SharedWithUserId == userId && fs.IsDeleted)
            .ToListAsync();

        var deletedSharedFiles = await _context.FileShares
            .Include(fs => fs.File)
            .Where(fs => fs.SharedWithUserId == userId && fs.IsDeleted)
            .ToListAsync();

        foreach (var folder in deletedFolders)
        {
            foreach (var file in folder.Files)
            {
                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }
                _context.Files.Remove(file);
            }
        }

        foreach (var sharedFolder in deletedSharedFolders)
        {
            if (sharedFolder.Folder != null)
            {
                var folderPath = sharedFolder.ShareFolderPath;
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }

                var sharedFiles = await _context.FileShares
                    .Where(fs => fs.File.FolderId == sharedFolder.Folder.Id)
                    .ToListAsync();

                foreach (var sharedFile in sharedFiles)
                {
                    if (System.IO.File.Exists(sharedFile.ShareFilePath))
                    {
                        System.IO.File.Delete(sharedFile.ShareFilePath);
                    }
                    _context.FileShares.Remove(sharedFile);
                }
            }
        }

        foreach (var file in deletedFiles)
        {
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
            _context.Files.Remove(file);
        }

        foreach (var sharedFile in deletedSharedFiles)
        {
            if (System.IO.File.Exists(sharedFile.ShareFilePath))
            {
                System.IO.File.Delete(sharedFile.ShareFilePath);
            }
            _context.FileShares.Remove(sharedFile);
        }

        foreach (var folder in deletedFolders)
        {
            var folderPath = Path.Combine(trashBasePath, folder.Name);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            _context.Folders.Remove(folder);
        }

        foreach (var sharedFolder in deletedSharedFolders)
        {
            if (Directory.Exists(sharedFolder.ShareFolderPath))
            {
                Directory.Delete(sharedFolder.ShareFolderPath, true);
            }
            _context.FolderShares.Remove(sharedFolder);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "All items in trash, including shared files and folders, deleted successfully." });
    }

    [HttpPost]
    [Route("Trash/MoveImportantSharedFolderToTrash/{folderShareId}")]
    public async Task<IActionResult> MoveImportantSharedFolderToTrash(int folderShareId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            Console.WriteLine("Unauthorized access attempt.");
            return Unauthorized("User not authenticated.");
        }

        Console.WriteLine($"Received folderShareId: {folderShareId}, UserId: {userId}");

        var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "UserFolders", userId.ToString());
        var trashFolderPath = Path.Combine(basePath, "Trash");

        if (!Directory.Exists(trashFolderPath))
        {
            Directory.CreateDirectory(trashFolderPath);
        }

        var importantSharedFolder = await _context.FolderShares
            .Include(fs => fs.Folder)
            .FirstOrDefaultAsync(fs => fs.Id == folderShareId
                                       && fs.SharedWithUserId == userId
                                       && fs.IsImportant
                                       && !fs.IsDeleted);

        if (importantSharedFolder == null)
        {
            Console.WriteLine("Important shared folder not found in database.");
            return NotFound("Important shared folder not found.");
        }

        Console.WriteLine($"Important shared folder found: ID = {importantSharedFolder.Id}, Path = {importantSharedFolder.ShareFolderPath}");

        var sharedFolderPath = importantSharedFolder.ShareFolderPath;

        if (string.IsNullOrEmpty(sharedFolderPath) || !Directory.Exists(sharedFolderPath))
        {
            Console.WriteLine($"Important shared folder path does not exist or is null: {sharedFolderPath}");
            return NotFound("Important shared folder path does not exist.");
        }

        var trashPath = Path.Combine(trashFolderPath, importantSharedFolder.Folder.Name);

        try
        {
            Directory.Move(sharedFolderPath, trashPath);

            importantSharedFolder.IsDeleted = true;
            importantSharedFolder.ShareFolderPath = trashPath;

            await _context.SaveChangesAsync();
            Console.WriteLine($"Important shared folder moved to Trash: {trashPath}");
            return Ok(new { message = "Important shared folder moved to Trash." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving important shared folder: {ex.Message}");
            return StatusCode(500, $"Error moving important shared folder: {ex.Message}");
        }
    }

}

