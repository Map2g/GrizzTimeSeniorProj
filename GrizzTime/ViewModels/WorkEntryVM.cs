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
        public WorkEntry()
        {
            this.possibleProjects = new List<Project>();
            this.possibleTasks = new List<GrizzTime.Models.task>();
        }

        public int WorkEntryID { get; set; }

        public int EmpID { get; set; }

        public string EmpName { get; set; }

        public int ProjID { get; set; }

        public string ProjName { get; set; }

        //Day of the week
        public String WorkDate { get; set; }

        //Start day of timesheet this workentry belongs to
        public DateTime WorkTSDate { get; set; }

        public decimal WorkHours { get; set; }

        public int TimeSheetID { get; set; }

        public int TaskID { get; set; }

        public string TaskName { get; set; }

        public List<Project> possibleProjects { get; set; }
        public List<GrizzTime.Models.task> possibleTasks { get; set; }

        //public virtual task task { get; set; }

    }

}