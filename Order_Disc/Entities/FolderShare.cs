using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Order_Disc.Entities
{
    
        public class FolderShare
        {
            public int Id { get; set; }

            [Required]
            public int FolderId { get; set; }

            [ForeignKey("FolderId")]
            public FolderEntity Folder { get; set; } = null!;

            [Required]
            public int SharedWithUserId { get; set; }

            [ForeignKey("SharedWithUserId")]
            public UserAccounts SharedWithUser { get; set; } = null!;

            [Required]
            public int SharedByUserId { get; set; }

            [ForeignKey("SharedByUserId")]
            public UserAccounts SharedByUser { get; set; } = null!;
            public string ShareFolderPath { get; set; } = string.Empty;
            public bool IsDeleted { get; set; } = false;
            public bool IsImportant { get; set; } = false;
            public string OrginalFolderPath { get; set; }
            [Required]
            public string AccessLevel { get; set; } = "Read";
            public virtual ICollection<FileEntity> Files { get; set; }
    }
    

}
