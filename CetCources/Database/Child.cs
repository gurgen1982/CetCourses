//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CetCources.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Child
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Child()
        {
            this.FreeHours = new HashSet<FreeHour>();
        }
    
        public int ChildId { get; set; }
        public string ParentId { get; set; }
        public string FullName { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public Nullable<int> YearId { get; set; }
        public string SchoolId { get; set; }
        public int ClassNo { get; set; }
        public Nullable<int> FreqId { get; set; }
        public Nullable<int> GroupId { get; set; }
        public int EduLevel { get; set; }
        public System.DateTime CreationDate { get; set; }
        public bool Inactive { get; set; }
        public string Comment { get; set; }
    
        public virtual CourseFrequency CourseFrequency { get; set; }
        public virtual Group Group { get; set; }
        public virtual YearGroup YearGroup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FreeHour> FreeHours { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
