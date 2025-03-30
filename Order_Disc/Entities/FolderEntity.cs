using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Order_Disc.Entities;

namespace Order_Disc.Entities;
public class FolderEntity
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Folder name is required!")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int UserAccountId { get; set; }

    [ForeignKey("UserAccountId")]
    public UserAccounts? User { get; set; }

    [Required]
    public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();

    public ICollection<FolderShare> SharedWithUsers { get; set; } = new List<FolderShare>();

    public string DefaultAccessLevel { get; set; } = "Read";

    public bool IsDeleted { get; set; } = false;

    [Required]
    public string Path { get; set; } = string.Empty;

    [Required(ErrorMessage = "File path is required.")]
    public string OrginalFolderPath { get; set; }

    public bool IsImportant { get; set; }

    [NotMapped]
    public bool IsShared { get; set; }
    public ICollection<FolderShare> SharedFolders { get; set; } = new List<FolderShare>();
}
