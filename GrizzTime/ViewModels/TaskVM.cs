using GrizzTime.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class Task
    {
        public int TaskID { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "This field is required. ")]
        public string TaskName { get; set; }

        [Display(Name = "Billable Rate")]
        public decimal BillableRate { get; set; }

        public Project BelongsToProject { get; set; }

        public int ProjID { get; set; }

        //---------To be used in Employee Invoice-------------------
        public decimal EmpTaskHours { get; set; }

        public decimal EmpTaskAmt { get; set; }

        //----------------------------------------------------------
    }
}