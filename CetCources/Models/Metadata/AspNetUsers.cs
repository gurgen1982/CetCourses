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
        public string FullName { get; set; }

        [Display(Name = "PhoneNumber", ResourceType = typeof(ChildRes))]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email", ResourceType = typeof(ChildRes))]
        public string Email { get; set; }
    }
}