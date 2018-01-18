using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CetCources.Models
{
    public class DayHoursModel
    {
        public int HourId { get; set; }
        public int DayOfWeek { get; set; }
        public string Hours { get; set; }
        public bool IsChecked { get; set; }
    }
}