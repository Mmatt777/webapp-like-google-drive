namespace Order_Disc.Models;

public class FolderVM
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string? OwnerName { get; set; } 
    public int? FolderShareId { get; set; }
    public string Path { get; set; } = string.Empty; 
    public int UserAccountId { get; set; }
    public bool IsImportant { get; set; }
    public bool IsShared { get; set; }
    public bool IsDeleted { get; set; }
    public string OrginalFolderPath { get; set; }   
    public List<FilesVM> Files { get; set; } = new List<FilesVM>();
}
