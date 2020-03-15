using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class Expensesheet
    {
        public int ExpSheetID { get; set; }

        public int PayrollCycleID { get; set; }

        public int EmpID { get; set; }

        [Display(Name = "Week")]
        public System.DateTime Week { get; set; }

        public string ExpSheetStatus { get; set; }

        public decimal ExpSheetTotalAmt { get; set; }

        public Nullable<System.DateTime> ExpSheetSubmitTime { get; set; }

        public Nullable<System.DateTime> ExpSheetApproveTime { get; set; }

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