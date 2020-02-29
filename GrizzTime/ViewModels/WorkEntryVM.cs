using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class WorkEntry
    {
        public int WorkEntryID { get; set; }

        public int EmpID { get; set; }

        public string EmpName { get; set; }

        public int ProjID { get; set; }

        public int ProjName { get; set; }

        //Day of the week
        public String WorkDate { get; set; }

        public decimal WorkHours { get; set; }

        public int TimeSheetID { get; set; }

        public int TaskID { get; set; }

        public string TaskName { get; set; }


        //public virtual timesheet timesheet { get; set; }

        //public virtual task task { get; set; }

        public static List<SelectListItem> TaskTypes = new List<SelectListItem>()
        {
            new SelectListItem() {Text="Administration", Value="Administration" },
            new SelectListItem() {Text="Development", Value="Development" },
            new SelectListItem() {Text="Helpdesk & Support", Value="Helpdesk & Support" },
            new SelectListItem() {Text="Management", Value="Management" },
            new SelectListItem() {Text="Quality Assurance", Value="Quality Assurance" },
            new SelectListItem() {Text="Design", Value="Design" },
            new SelectListItem() {Text="Requirements Capture", Value="Requirements Capture" },
            new SelectListItem() {Text="Accounting", Value="Accounting" },
            new SelectListItem() {Text="Business Development", Value="Business Development" },
            new SelectListItem() {Text="Research", Value="Research" },
            new SelectListItem() {Text="Deployment", Value="Deployment" },
            new SelectListItem() {Text="Maintenance", Value="Maintenance" },
        };
    }

}