using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Order_Disc.Entities
{
    public class FileShareEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FileId { get; set; }

        [ForeignKey("FileId")]
        public FileEntity File { get; set; }

        [Required]
        public int SharedWithUserId { get; set; }

        [ForeignKey("SharedWithUserId")]
        public UserAccounts SharedWithUser { get; set; }

        [Required]
        public int SharedByUserId { get; set; }

        [ForeignKey("SharedByUserId")]
        public UserAccounts? SharedByUser { get; set; }

        [Required(ErrorMessage = "File path is required.")]
        public string OrginalFilePath { get; set; } = string.Empty;

        public string ShareFilePath { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public bool IsImportant { get; set; } = false;

        public string AccessLevel { get; set; } = "read";
    }
}
