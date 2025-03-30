namespace Order_Disc.Models;
public class ImportantVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } 
    public string OwnerName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int OwnerId { get; set; } 
    public string OwnerUserName { get; set; } 
    public string OwnerLastName { get; set; }
    public string Path { get; set; }
    public int folderShareId { get; set; }
    public List<FolderVM> SubFolders { get; set; } = new List<FolderVM>();
    public List<FilesVM> Files { get; set; } = new List<FilesVM>();
}
