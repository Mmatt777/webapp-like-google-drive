using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Order_Disc.Entities
{
    public class FileEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "File name is required.")]
        [MaxLength(255)]
        public string FileName { get; set; } = null!;

        [Required(ErrorMessage = "File path is required.")]
        public string FilePath { get; set; } = null!;

        [Required(ErrorMessage = "Original file path is required.")]
        public string OrginalFilePath { get; set; } = null!;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [Range(1, long.MaxValue, ErrorMessage = "File size must be greater than 0 bytes.")]
        public long SizeInBytes { get; set; }

        [Required]
        public int FolderId { get; set; }

        [ForeignKey("FolderId")]
        public FolderEntity Folder { get; set; } = null!;

        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
        public bool IsImportant { get; set; }

        [NotMapped]
        public bool IsShared { get; set; }

        public virtual ICollection<FileShareEntity> SharedFiles { get; set; } = new List<FileShareEntity>();
    }
}
