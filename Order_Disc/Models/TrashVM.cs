using Order_Disc.Entities;

namespace Order_Disc.Models
{
    public class TrashVM
    {
        public List<FolderVM> SubFolders { get; set; } = new List<FolderVM>();
        public List<FilesVM> Files { get; set; } = new List<FilesVM>();
    }
}
