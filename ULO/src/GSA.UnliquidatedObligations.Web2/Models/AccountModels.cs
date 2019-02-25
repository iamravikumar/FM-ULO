using System.ComponentModel.DataAnnotations;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class DevLoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class LoginViewModel
    {
        public bool ShowUserNotInDBMessage { get; set; }

        public LoginViewModel(bool showUserNotInDbMessage = false)
        {
            ShowUserNotInDBMessage = showUserNotInDbMessage;
        }
    }
}
