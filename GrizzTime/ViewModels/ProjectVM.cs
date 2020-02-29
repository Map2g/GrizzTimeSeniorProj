﻿using GrizzTime.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
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

        [Display(Name = "End Date: ")]
        public DateTime? ProjEndDate { get; set; }

        //Contract this project belongs to. 
        [Display(Name = "Belongs to: ")]
        [Required(ErrorMessage = "This field is required. ")]
        public string ConID { get; set; }

        //Project manager incharge of this
        [Display(Name = "Project Manager: ")]
        [Required(ErrorMessage = "This field is required. ")]
        public string ProjManID { get; set; }

        [Display(Name = "Project Manager: ")]
        public string ProjManName { get; set; }

        [Display(Name = "Contract: ")]
        public string ContractName { get; set; }

        [Display(Name = "Mark as ended: ")]
        public bool IsEnded { get; set; }

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

        // project.employee.EmpFName + " " + project.employee.EmpLName seems to work fine...?
        public static string GetProjManName(int id)
        {
            //string query = "SELECT * FROM grizztime.employee WHERE UserID = @id";
            Entities dc = new Entities();
            employee projMan = dc.employees.Find(id);
            return (projMan.EmpFName + " " + projMan.EmpLName);
        }

        //Get employees working on a project
        public static List<Employee> GetEmployees(int id)
        {
            using (Entities dc = new Entities())
            {
                //get employees for this project
                var thisProjEmmployees = (from e in dc.employees
                                      join p in dc.employee_project
                                      on e.UserID equals p.EmpID
                                      join q in dc.projects
                                      on p.ProjID equals q.ProjID
                                      where q.ProjID == id
                                      select new { e.EmpFName, e.EmpLName, e.UserID, e.EmpType }
                            ).ToList();

                List<Employee> tryIt = new List<Employee>();

                foreach (var item in thisProjEmmployees)
                {
                    tryIt.Add(new Employee()
                    {
                        EmpFName = item.EmpFName,
                        EmpLName = item.EmpLName,
                        UserID = item.UserID,
                        EmpType = item.EmpType,
                    });
                }
                return tryIt;
            }
        }

        public static List<Project> BusProjList(int id)
        {
            using (Entities dc = new Entities())
            {
                //Get contracts belonging to business
                int busContracts = (from c in dc.contracts
                                    where c.BusID == id
                                    select c.ConID).First();

                //get projects in those contracts
                var thisBusProjects = (from p in dc.projects
                                       join e in dc.employees
                                       on p.ProjManID equals e.UserID
                                       join c in dc.contracts
                                       on p.ConID equals c.ConID
                                       where c.ConID == busContracts
                                       select new { p.ProjName, p.ProjDesc, p.ProjStartDate, p.ProjEndDate, p.ProjStatus, p.ProjID, e.EmpFName, e.EmpLName, c.ConName }
                                ).ToList();

                List<Project> tryIt = new List<Project>();

                foreach (var item in thisBusProjects)
                {
                    tryIt.Add(new Project()
                    {
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

        public static List<Project> PMProjList(int id)
        {
            using (Entities dc = new Entities())
            {
                //get projects for this project manager
                var thisPMProjects = (from p in dc.projects
                                      join c in dc.contracts
                                      on p.ConID equals c.ConID
                                      where p.ProjManID == id
                                      select new { p.ProjName, p.ProjDesc, p.ProjStartDate, p.ProjEndDate, p.ProjStatus, p.ProjID, c.ConName }
                            ).ToList();

                List<Project> tryIt = new List<Project>();

                foreach (var item in thisPMProjects)
                {
                    tryIt.Add(new Project()
                    {
                        ProjName = item.ProjName,
                        ProjDesc = item.ProjDesc,
                        ProjStartDate = item.ProjStartDate,
                        ProjEndDate = item.ProjEndDate,
                        ContractName = item.ConName,
                        ProjStatus = item.ProjStatus,
                        ProjID = item.ProjID,
                    });
                }
                return tryIt;
            }
        }

        public static List<task> GetTasks(int id)
        {
            using (Entities dc = new Entities())
            {
                //get projects for this project manager
                var thisProjectTasks = (from p in dc.projects
                                      join t in dc.tasks
                                      on p.ProjID equals t.ProjID
                                      where p.ProjID == id
                                      select new { p.ProjID, p.ProjName, t.TaskName, t.TaskID, t.BillableRate }
                            ).ToList();

                List<task> tryIt = new List<task>();

                foreach (var item in thisProjectTasks)
                {
                    tryIt.Add(new task()
                    {
                        TaskName = item.TaskName,
                        BillableRate = item.BillableRate,
                        TaskID = item.TaskID,
                        ProjID = item.ProjID,
                    });
                }

                return tryIt;
            }
        }
    }
}