using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Order_Disc.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    public class UserAccounts
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(40, ErrorMessage = "Max 40 characters allowed")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(40, ErrorMessage = "Max 40 characters allowed")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(100, ErrorMessage = "Max 100 characters allowed")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed")]
        public string? Password { get; set; }

        public ICollection<FolderEntity> Folders { get; set; } = new List<FolderEntity>();
    }
}
