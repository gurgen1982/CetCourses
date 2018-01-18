using System;
using System.ComponentModel.DataAnnotations;
using Resources;
using System.ComponentModel;

namespace CetCources.Database
{
    [MetadataType(typeof(CourseFrequencyMetadata))]
    public partial class CourseFrequency
    {
    }

    public class CourseFrequencyMetadata
    {
        [Range(0, int.MaxValue)]
        [Display(Name = "FrequencyDescription", ResourceType = typeof(FrequencyRes))]
        public int FreqId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(maximumLength: 50, MinimumLength = 5, ErrorMessageResourceName = "FrequencyDescriptionMinMaxMessage", ErrorMessageResourceType = typeof(FrequencyRes))]
        [Display(Name = "FrequencyDescription", ResourceType = typeof(FrequencyRes))]
        public string FrequencyDescription { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [Display(Name = "DaysCount", ResourceType = typeof(FrequencyRes))]
        public Nullable<short> DaysCount { get; set; }

        [Display(Name = "Inactive", ResourceType = typeof(FrequencyRes))]
        public bool Inactive { get; set; }
    }
}