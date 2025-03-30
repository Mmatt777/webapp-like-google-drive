using System.ComponentModel.DataAnnotations;

namespace Order_Disc.Models;

public class RegistrationVM
{
    [Required(ErrorMessage = "This field is required")]
    [MaxLength(40, ErrorMessage = "Maximum 40 characters allowed")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MaxLength(40, ErrorMessage = "Maximum 40 characters allowed")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MaxLength(20, ErrorMessage = "Maximum 20 characters allowed")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MaxLength(100, ErrorMessage = "Maximum 100 characters allowed")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Allowed minimum 5 and maximum 20 characters")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Required(ErrorMessage = "This field is required")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
}
