
using Resources;
using System.ComponentModel.DataAnnotations;

namespace CetCources.Database
{
    [MetadataType(typeof(YearGroupMetadata))]
    public partial class YearGroup
    {
    }

    public partial class YearGroupMetadata
    {
        [Required]
        [StringLength(50, MinimumLength = 5)]
        [Display(Name = "GroupName", ResourceType = typeof(YearGroupRes))]
        public string GroupName { get; set; }

        [Required]
        [Range(3, 18)]
        [Display(Name = "From", ResourceType = typeof(YearGroupRes))]
        public int From { get; set; }

        [Required]
        [Range(3, 18)]
        [IsGreater("From")]
        [Display(Name = "To", ResourceType = typeof(YearGroupRes))]
        public int To { get; set; }
    }
}
