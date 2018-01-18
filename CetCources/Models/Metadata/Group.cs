
using Resources;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CetCources.Database
{
    [MetadataType(typeof(GroupMetadata))]
    public partial class Group
    {
        [Display(Name = "MailMessage", ResourceType = typeof(GroupRes))]
        [StringLength(maximumLength: 1000, MinimumLength = 0)]
        public string MailMessage { get; set; }
    }

    public partial class GroupMetadata
    {
        public int GroupId { get; set; }
        [Required]
        [Display(Name = "YearId", ResourceType = typeof(GroupRes))]
        public Nullable<int> YearId { get; set; }
        [Required]
        [Display(Name = "FreqId", ResourceType = typeof(GroupRes))]
        public Nullable<int> FreqId { get; set; }

        [Required]
        [Display(Name = "GroupName", ResourceType = typeof(GroupRes))]
        [StringLength(maximumLength: 50, MinimumLength = 5)]
        public string GroupName { get; set; }

        [Required]
        [Range(1, 500)]
        [Display(Name = "PersonCount", ResourceType = typeof(GroupRes))]
        public Nullable<int> PersonCount { get; set; }

        [Display(Name = "Inactive", ResourceType = typeof(GroupRes))]
        public bool Inactive { get; set; }
        public Nullable<int> PlaceId { get; set; }

        [Display(Name = "Sunday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Sunday { get; set; }

        [Display(Name = "Monday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Monday { get; set; }

        [Display(Name = "Tuesday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Tuesday { get; set; }

        [Display(Name = "Wednesday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Wednesday { get; set; }

        [Display(Name = "Thursday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Thursday { get; set; }

        [Display(Name = "Friday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Friday { get; set; }

        [Display(Name = "Saturday", ResourceType = typeof(CommonRes))]
        public Nullable<int> Saturday { get; set; }

        [Display(Name = "EduLevel", ResourceType = typeof(GroupRes))]
        public Nullable<int> EduLevel { get; set; }

        [Display(Name = "Trainer", ResourceType = typeof(GroupRes))]
        public string Trainer { get; set; }
    }
}