namespace Order_Disc.Models
{
    public class SharedFolderVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int FolderId { get; set; }
        public int FolderShareId { get; set; } 
        public string SharedByFirstName { get; set; } = null!;
        public string SharedByLastName { get; set; } = null!;
        public string SharedWithFirstName { get; set; } = null!;
        public string SharedWithLastName { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public List<FileShareVM> Files { get; set; } = new List<FileShareVM>();
    }
}
