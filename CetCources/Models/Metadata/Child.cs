using System;
using System.ComponentModel.DataAnnotations;
using Resources;
using System.ComponentModel;
using CetCources.Models.Base;

namespace CetCources.Database
{
    [MetadataType(typeof(ChildMetadata))]
    public partial class Child : ModelBase
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [Display(Name = "BirthDate", ResourceType = typeof(ChildRes))]
        ///[DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public string BirthDateString
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(BirthDate, System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                }
                catch
                {
                    return BirthDate?.Day + "/" + BirthDate?.Month + "/" + BirthDate?.Year;
                }
                //return BirthDate.ToString("dd/MM/yyyy");
            }
            set
            {
                DateTime bd;
                DateTime.TryParseExact(value, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out bd);
                if (bd != null && bd != DateTime.MinValue) BirthDate = bd;
                else
                {
                    var s = new string[0];
                    if (value.Contains("."))
                    {
                        s = value.Split('.');
                    }
                    else if (value.Contains("/"))
                    {
                        s = value.Split('/');
                    }
                    else if (value.Contains("-"))
                    {
                        s = value.Split('-');
                    }
                    try
                    {
                        if (s.Length == 3)
                        {
                            BirthDate = new DateTime(Convert.ToInt32(s[2]), Convert.ToInt32(s[1]), Convert.ToInt32(s[0]));
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        BirthDate = DateTime.Now.AddYears(-18);
                    }
                }
                //System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("us-EN");
                //BirthDate = Convert.ToDateTime(value, culture);
            }
        }
        
        [Display(Name = "ParentInfo", ResourceType = typeof(ChildRes))]
        public string UserId { get; set; }
    }

    public class ChildMetadata
    {
        [Range(0, int.MaxValue)]
        [Display(Name = "ChildId", ResourceType = typeof(ChildRes))]
        public int ChildId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [StringLength(maximumLength: 50, MinimumLength = 5, ErrorMessageResourceName = "FullNameMinMaxMessage", ErrorMessageResourceType = typeof(ChildRes))]
        [Display(Name = "FullName", ResourceType = typeof(ChildRes))]
        public string FullName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [Display(Name = "YearId", ResourceType = typeof(ChildRes))]
        public Nullable<int> YearId { get; set; }

        //[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [Display(Name = "SchoolId", ResourceType = typeof(ChildRes))]
        public int SchoolId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [Range(1, 12, ErrorMessageResourceName = "ClassNoMinMaxMessage", ErrorMessageResourceType = typeof(ChildRes))]
        [Display(Name = "ClassNo", ResourceType = typeof(ChildRes))]
        public int ClassNo { get; set; }

        //[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(CommonRes))]
        [Display(Name = "FreqId", ResourceType = typeof(ChildRes))]
        public Nullable<int> FreqId { get; set; }

        [Display(Name = "GroupId", ResourceType = typeof(ChildRes))]
        public Nullable<int> GroupId { get; set; }

        [Display(Name = "EduLevel", ResourceType = typeof(ChildRes))]
        public int EduLevel { get; set; }

        [Display(Name = "Inactive", ResourceType = typeof(ChildRes))]
        public bool Inactive { get; set; }
        [Display(Name = "Comment", ResourceType = typeof(ChildRes))]
        public string Comment { get; set; }
        [Display(Name = "CreationDate", ResourceType = typeof(ChildRes))]
        public System.DateTime CreationDate { get; set; }
    }
}