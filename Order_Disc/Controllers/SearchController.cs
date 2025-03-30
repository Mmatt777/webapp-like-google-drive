using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order_Disc.Entities;
using Order_Disc.Models;
using System.Security.Claims;

namespace Order_Disc.Controllers
{
    public class SearchController:Controller
    {
        private readonly AffDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SearchController(AffDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Route("Search/GetSearchResults")]
        public async Task<IActionResult> GetSearchResults(string query)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userFolders = await _context.Folders
                .Where(f => f.UserAccountId == userId && f.Name.Contains(query))
                .Select(f => new FolderVM
                {
                    Id = f.Id,
                    Name = f.Name,
                    Path = f.Path,
                    CreatedDate = f.CreatedDate,
                    IsShared = false
                })
                .ToListAsync();

            var sharedFolders = await _context.FolderShares
                .Include(fs => fs.Folder)
                .Where(fs => fs.SharedWithUserId == userId && fs.Folder.Name.Contains(query))
                .Select(fs => new FolderVM
                {
                    Id = fs.Folder.Id,
                    FolderShareId = fs.Id,
                    Name = fs.Folder.Name,
                    Path = fs.Folder.Path,
                    CreatedDate = fs.Folder.CreatedDate,
                    IsShared = true
                })
                .ToListAsync();

            var userFiles = await _context.Files
                .Where(f => f.Folder.UserAccountId == userId && f.FileName.Contains(query))
                .Select(f => new FilesVM
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    SizeInBytes = f.SizeInBytes,
                    UploadDate = f.UploadDate,
                    IsShared = false
                })
                .ToListAsync();

            var sharedFiles = await _context.FileShares
                .Include(fs => fs.File)
                .Where(fs => fs.SharedWithUserId == userId && fs.File.FileName.Contains(query))
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

            var searchVM = new SearchVM
            {
                SubFolders = userFolders,
                Files = userFiles
            };

            return PartialView("~/Views/Folder/Results.cshtml", searchVM);
        }
    }

}

