using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class Timesheet
    {
        public int TimeSheetID { get; set; }

        public int PayrollCycleID { get; set; }

        public int EmpID { get; set; }

        public string TimeSheetStatus { get; set; }

        public decimal TimeSheetTotalHr { get; set; }

        public Nullable<System.DateTime> TimeSheetSubmitTime { get; set; }

        public Nullable<System.DateTime> TimeSheetApproveTime { get; set; }

        public string PayrollCycleYear { get; set; }

        public DateTime PayrollCycleStart { get; set; }

        public List<WorkEntry> TimesheetWorkEntries { get; set; }


        public static List<String> TaskTypes = new List<String>()
        {
            //new SelectListItem() {Text="Administration", Value="Administration" },
            //new SelectListItem() {Text="Development", Value="Development" },
            //new SelectListItem() {Text="Helpdesk & Support", Value="Helpdesk & Support" },
            //new SelectListItem() {Text="Management", Value="Management" },
            //new SelectListItem() {Text="Quality Assurance", Value="Quality Assurance" },
            //new SelectListItem() {Text="Design", Value="Design" },
            //new SelectListItem() {Text="Requirements Capture", Value="Requirements Capture" },
            //new SelectListItem() {Text="Accounting", Value="Accounting" },
            //new SelectListItem() {Text="Business Development", Value="Business Development" },
            //new SelectListItem() {Text="Research", Value="Research" },
            //new SelectListItem() {Text="Deployment", Value="Deployment" },
            //new SelectListItem() {Text="Maintenance", Value="Maintenance" },
            "Administration",
            "Development",
            "Helpdesk & Support",
            "Management",
            "Quality Assurance",
            "Design",
            "Requirements Capture",
            "Accounting",
            "Business Development",
            "Research",
            "Deployment",
            "Maintenance"
        };
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}