using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Order_Disc.Models;

public class LoginVM
{
    [Required(ErrorMessage = "This field is required")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Allowed minimum 5 and maximum 20 characters")]
    [DisplayName("Use Email or Username")]
    public string? UserNameOrEmail { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Allowed minimum 5 and maximum 20 characters")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
