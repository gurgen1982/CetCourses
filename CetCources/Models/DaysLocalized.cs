using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CetCources.Models
{
    using c = Resources.CommonRes;

    public static class Days
    {
        ///static DaysLocalized days = new DaysLocalized();
        public static DaysLocalized Day
        {
            get
            {
                return new DaysLocalized();
            }
        }
    }
    
    public class DaysLocalized
    {
        private string[] Days = { c.Sunday, c.Monday, c.Tuesday, c.Wednesday, c.Thursday, c.Friday, c.Saturday };
        
        public string this [int ind]
        {
            get
            {
                if (ind > -1 && ind < 8)
                    return Days[ind];
                else
                {
                    return null;
                }
            }
        }
    }
}