using GrizzTime.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace GrizzTime.BusinessLogic
{
    public class Project
    {

        public int ProjID { get; set; }

        [Display(Name = "Name: ")]
        [Required(ErrorMessage = "This field is required. ")]
        public string ProjName { get; set; }

        [Display(Name = "Description: ")]
        public string ProjDesc { get; set; }

        [Display(Name = "Start Date: ")]
        [Required(ErrorMessage = "This field is required. ")]
        public DateTime ProjStartDate { get; set; }

        public string ProjStatus { get; set; }

        public DateTime? ProjEndDate { get; set; }

        //Contract this project belongs to. 
        [Display(Name = "Belongs to: ")]
        [Required(ErrorMessage = "This field is required. ")]
        public string ConID { get; set; }

        //Project manager incharge of this
        [Display(Name = "Project Manager: ")]
        [Required(ErrorMessage = "This field is required. ")]
        public string ProjManID { get; set; }

        public string ProjManName { get; set; }

        public string ContractName { get; set; }

        //Gets all the projects belonging to a specific contract.
        public static List<SelectListItem> ConProjList(string id)
        {
            string query = "SELECT * FROM grizztime.project WHERE ConID = @id";

            Entities dc = new Entities();
            IEnumerable<project> thisContractProjects = dc.projects.SqlQuery(query, new SqlParameter("@id", id));

            List<SelectListItem> AllProjects = new List<SelectListItem>();

            foreach (var item in thisContractProjects)
            {
                AllProjects.Add(new SelectListItem() { Text = item.ProjName, Value = item.ProjID.ToString() });
            };

            return AllProjects;
        }

        public static string GetProjManName(int id)
        {
            //string query = "SELECT * FROM grizztime.employee WHERE UserID = @id";
            Entities dc = new Entities();
            employee projMan = dc.employees.Find(id);
            return (projMan.EmpFName + " " + projMan.EmpLName);
        }

        public static List<Project> BusProjList(int id)
        {
            using (Entities dc = new Entities())
            {
                //Get contracts belonging to business
                //int contracts = (from c in dc.contracts
                //                where c.BusID == id
                //               select c.ConID).First();

                //get projects in those contracts
                var thisProject = (from p in dc.projects
                                   join e in dc.employees
                                   on p.ProjManID equals e.UserID
                                   join c in dc.contracts
                                   on p.ConID equals c.ConID
                                   select new { p.ProjName, p.ProjDesc, p.ProjStartDate, p.ProjEndDate, p.ProjStatus, p.ProjID, e.EmpFName, e.EmpLName, c.ConName }
                                ).ToList();

                List<Project> tryIt = new List<Project>();
                
                foreach (var item in thisProject) {
                    tryIt.Add(new Project() {
                        ProjName = item.ProjName,
                        ProjDesc = item.ProjDesc,
                        ProjStartDate = item.ProjStartDate,
                        ProjEndDate = item.ProjEndDate,
                        ProjManName = item.EmpFName + " " + item.EmpLName,
                        ContractName = item.ConName,
                        ProjStatus = item.ProjStatus,
                        ProjID = item.ProjID,
                        });
                }
                return tryIt;
            }
                
        }
    }
}        