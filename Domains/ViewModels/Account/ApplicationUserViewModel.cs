using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    public class ApplicationUserLogInViewModel
    {
        [Required(ErrorMessage = "Email / username is required")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string IpAddress { get; set; }
        public bool RememberMe { get; set; }

    } 
    public class ApplicationUserLevelLogInViewModel
    {
        [Required(ErrorMessage = "Email / username is required")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
    public class ApplicationUserRegistrationViewModel : ApplicationUserLogInViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }
        public string NickName { get; set; }
        public string LocalName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in proper format")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Confirm Email is required")]
        public string ConfirmEmail { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Password and confirm password values do not match")]
        public string ConfirmPassword { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string EmailConfirmationUrl { get; set; }
        public DateTime DOB { get; set; }
        public List<string> Roles { get; set; }
        public bool IsInternal { get; set; }

    }

    public class ApplicationUserUpdateViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }
        public string NickName { get; set; }
        public string LocalName { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public DateTime DOB { get; set; }
        public string UserId { get; set; }

    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email / username is required")]
        [EmailAddress]
        public string Email { get; set; }
        public string ResetUrl { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Token { get; set; }
        public string ReturnUrl { get; set; }
    }
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }

    }
    public class SocialMediaRegistrationViewModel
    {
        public string Token { get; set; }
    }
    public class ConfirmEmailViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        public string ReturnUrl { get; set; }
    }
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string CalledName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
        public bool EnableLocalLogin { get; set; } = true;
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;

    }
}
