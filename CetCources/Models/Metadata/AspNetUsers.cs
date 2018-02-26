using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CetCources.Database
{
    [MetadataType(typeof(AspNetUserMetadata))]
    public partial class AspNetUser
    {
    }

    public class AspNetUserMetadata
    {
        [Display(Name = "FullName", ResourceType = typeof(ChildRes))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(50, ErrorMessageResourceName = "FullNameMinMaxMessage", ErrorMessageResourceType = typeof(ChildRes), MinimumLength = 5)]
        public string FullName { get; set; }

        [Display(Name = "PhoneNumber", ResourceType = typeof(ChildRes))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(50, ErrorMessageResourceName = "PhoneNumberMinMaxMessage", ErrorMessageResourceType = typeof(AccountRes), MinimumLength = 6)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email", ResourceType = typeof(ChildRes))]
        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Comments", ResourceType = typeof(AccountRes))]
        public string Comments { get; set; }
    }
}