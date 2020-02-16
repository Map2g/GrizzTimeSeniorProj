using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GrizzTime.Models;

namespace GrizzTime.BusinessLogic
{
    public class Employee
    {

        public int UserID { get; set;  }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "This field is required. ")]
        [EmailAddress(ErrorMessage = "Invalid email address. ")]
        public string UserEmail { get; set; }

        [Display(Name = "Password")]
        [MembershipPassword(
                MinRequiredNonAlphanumericCharacters = 1,
                MinRequiredPasswordLength = 8,
                ErrorMessage = "Your password must be at least 8 characters long and must contain at least one symbol (!, @, #, etc)."
        )]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "This field is required. ")]
        public string UserPW { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "This field is required. ")]
        public string EmpFName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "This field is required. ")]
        public string EmpLName { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "This field is required. ")]
        public string EmpType { get; set; }

        public int BusCode { get; }

        public string BusinessName { get; }

        [Display(Name = "Pay rate: ")]
        public decimal EmpPayRate { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number is invalid.")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Phone number is invalid. (e.g. 2485550712)")]
        public string EmpPhone { get; set; }

        public bool RememberMe { get; set; }

        [NotMapped] // Does not effect the database
        [DataType("Password")]
        [System.ComponentModel.DataAnnotations.Compare("UserPW", ErrorMessage = "The passwords do not match.")]       
        [Required(ErrorMessage = "Confirm password is required. ")]
        public string ConfirmPassword { get; set; }

        public static List<SelectListItem> JobTypes = new List<SelectListItem>()
        {
            new SelectListItem() {Text="President", Value="President" },
            new SelectListItem() {Text="Vice President", Value="Vice President" },
            new SelectListItem() {Text="CEO", Value="CEO" },
            new SelectListItem() {Text="CTO", Value="CTO" },
            new SelectListItem() {Text="CIO", Value="CIO" },
            new SelectListItem() {Text="CFO", Value="CFO" },
            new SelectListItem() {Text="Director", Value="Director" },
            new SelectListItem() {Text="Project Manager", Value="Project Manager" },
            new SelectListItem() {Text="Technical Lead", Value="Technical Lead" },
            new SelectListItem() {Text="Software Engineer", Value="Software Engineer" },
            new SelectListItem() {Text="Intern", Value="Intern" },
        };

        //Gets a list of employees in a specific business. ID should be UserID of a business
        public static List<SelectListItem> EmployeeList(string id)
        {
            string query = "SELECT * FROM grizztime.employee WHERE BusCode = @id";

            Entities dc = new Entities();
            IEnumerable<employee> thisBusinessEmployees = dc.employees.SqlQuery(query, new SqlParameter("@id", id));

            List<SelectListItem> AllEmployees = new List<SelectListItem>();

            foreach (var item in thisBusinessEmployees)
            {
                AllEmployees.Add(new SelectListItem() { Text = item.EmpFName + " " + item.EmpLName, Value = item.UserID.ToString() });
            };

            return AllEmployees;
        }

        public static List<Project> GetProjects(int id)
        {
            using (Entities dc = new Entities())
            {
                //get projects for this employee
                var thisPMProjects = (from e in dc.employees
                                      join p in dc.employee_project                                     
                                      on e.UserID equals p.EmpID
                                      join q in dc.projects
                                      on p.ProjID equals q.ProjID
                                      where e.UserID == id
                                      select new { q.ProjName, q.ProjID }
                            ).ToList();

                List<Project> tryIt = new List<Project>();

                foreach (var item in thisPMProjects)
                {
                    tryIt.Add(new Project()
                    {
                        ProjName = item.ProjName,
                        ProjID = item.ProjID,
                    });
                }
                return tryIt;
            }
        }

    }

}