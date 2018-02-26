using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CetCources.Models
{
    //public class ExternalLoginConfirmationViewModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }
    //}

    //public class ExternalLoginListViewModel
    //{
    //    public string ReturnUrl { get; set; }
    //}

    //public class SendCodeViewModel
    //{
    //    public string SelectedProvider { get; set; }
    //    public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    //    public string ReturnUrl { get; set; }
    //    public bool RememberMe { get; set; }
    //}

    //public class VerifyCodeViewModel
    //{
    //    [Required]
    //    public string Provider { get; set; }

    //    [Required]
    //    [Display(Name = "Code")]
    //    public string Code { get; set; }
    //    public string ReturnUrl { get; set; }

    //    [Display(Name = "Remember this browser?")]
    //    public bool RememberBrowser { get; set; }

    //    public bool RememberMe { get; set; }
    //}

    //public class ForgotViewModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }
    //}

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [EmailAddress(ErrorMessageResourceName = "NotValidMail", ErrorMessageResourceType = typeof(ManageRes))]
        [Display(Name = "Email", ResourceType = typeof(ManageRes))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(AccountRes))]
        public string Password { get; set; }

        [Display(Name = "Remember", ResourceType = typeof(AccountRes))]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [EmailAddress(ErrorMessageResourceName = "NotValidMail", ErrorMessageResourceType = typeof(ManageRes))]
        [Display(Name = "Email", ResourceType = typeof(ManageRes))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(100, ErrorMessageResourceName = "MustbeAtLeast", ErrorMessageResourceType = typeof(ManageRes), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password2", ResourceType = typeof(ManageRes))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(ManageRes))]
        [Compare("Password", ErrorMessageResourceName = "PasswordMismatch", ErrorMessageResourceType = typeof(ManageRes))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(50, ErrorMessageResourceName = "FullNameMinMaxMessage", ErrorMessageResourceType = typeof(ChildRes), MinimumLength = 5)]
        [Display(Name = "FullName", ResourceType = typeof(AccountRes))]
        public string FullName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(50, ErrorMessageResourceName = "PhoneNumberMinMaxMessage", ErrorMessageResourceType = typeof(AccountRes), MinimumLength = 6)]
        [Display(Name = "PhoneNumber", ResourceType = typeof(AccountRes))]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Comments", ResourceType = typeof(AccountRes))]
        public string Comments { get; set; }

        [Display(Name = "TermsAccepted", ResourceType = typeof(AccountRes))]
        //[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
       // [Range(typeof(bool), "true", "true", ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        public bool TermsAccepted { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [EmailAddress(ErrorMessageResourceName = "NotValidMail", ErrorMessageResourceType = typeof(ManageRes))]
        [Display(Name = "Email", ResourceType = typeof(ManageRes))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(100, ErrorMessageResourceName = "MustbeAtLeast", ErrorMessageResourceType = typeof(ManageRes), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password2", ResourceType =typeof(ManageRes))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(ManageRes))]
        [Compare("Password", ErrorMessageResourceName = "PasswordMismatch", ErrorMessageResourceType = typeof(ManageRes))]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [EmailAddress(ErrorMessageResourceName ="NotValidMail", ErrorMessageResourceType =typeof(ManageRes))]
        [Display(Name = "Email", ResourceType =typeof(ManageRes))]
        public string Email { get; set; }
    }
}
