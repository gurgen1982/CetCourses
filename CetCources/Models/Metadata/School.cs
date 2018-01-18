
using Resources;
using System.ComponentModel.DataAnnotations;

namespace CetCources.Database
{
    [MetadataType(typeof(SchoolMetadata))]
    public partial class School
    {
    }

    public partial class SchoolMetadata
    {
        [Display(Name = "SchoolName", ResourceType = typeof(ChildRes))]
        public string SchoolName { get; set; }
    }
}
