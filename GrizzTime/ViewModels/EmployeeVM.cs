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

namespace GrizzTime.ViewModels
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
        //[DataType(DataType.Password)]
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

        public int SupervisorID { get; set; }

        public Employee Supervisor { get; set; }

        [Display(Name = "Supervisor")]
        //[Required(ErrorMessage = "This field is required. ")]
        public string SupervisorName { get; set; }

        public int BusCode { get; set; }

        public string BusinessName { get; set; }

        [Display(Name = "Pay rate: ")]
        public decimal EmpPayRate { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(DataFormatString = "{0:###-###-####}")]
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


        //--------------Used in invoices--------------------------------------
        [Display(Name = "Earned")]
        [DataType(DataType.Currency)]
        public decimal YearTotalEarned { get; set; }

        [Display(Name = "Hours")]
        public decimal YearTotalHours { get; set; }

        [DataType(DataType.Currency)]
        public decimal YearTotalExpenseAmt { get; set; }

        public List<Project> EmployeeProjects { get; set; }

        public List<Timesheet> EmployeeTimesheets { get; set; }

        public List<Expensesheet> EmployeeExpensesheets { get; set; }

    public static List<Employee> EmployeeList(string id)
        {
            string query = "SELECT * FROM grizztime.employee WHERE BusCode = @id";

            Entities dc = new Entities();
            IEnumerable<employee> thisBusinessEmployees = dc.employees.SqlQuery(query, new SqlParameter("@id", id));

            List<Employee> AllEmployees = new List<Employee>();

            foreach (var item in thisBusinessEmployees)
            {
                AllEmployees.Add(new Employee()
                {
                    EmpFName = item.EmpFName,
                    EmpLName = item.EmpLName,
                    UserID = item.UserID,
                    EmpType = item.EmpType,
                    UserEmail = item.UserEmail,
                    EmpPhone = item.EmpPhone,                   
                    //add more if needed
                });               
            };

            return AllEmployees;

        }

        //takes employee id, returns all projects for an employee
        //Lots of good info in this method
        public static List<Project> GetProjects(int id)
        {
            using (Entities dc = new Entities())
            {
                //get projects for this employee
                var thisEmpProjects = (from e in dc.employees
                                      join p in dc.employee_project                                     
                                      on e.UserID equals p.EmpID
                                      join q in dc.projects
                                      on p.ProjID equals q.ProjID
                                      where e.UserID == id
                                      select new { q.ProjName, q.ProjDesc, q.ProjID, q.contract, q.workentries1, q.employee, q.ProjStartDate, q.ProjEndDate, q.ProjStatus, q.expenseentries}
                            ).ToList();
                
                List<Project> tryIt = new List<Project>();               

                //Will repeat for every one of this employee's projects.
                foreach (var item in thisEmpProjects)
                {
                    decimal projectHours = 0;
                    decimal projectAmt = 0;
                    decimal projectExp = 0;

                    List<Task> taskView = new List<Task>();
                    //Will repeat for every workentry for this project
                    foreach (var workitem in item.workentries1)
                    {
                        if (workitem.EmpID == id)
                        {
                            projectHours += workitem.WorkHours;

                            if (workitem.task.IsBillable == true)
                            {
                                projectAmt += (workitem.WorkHours * (decimal)workitem.task.BillableRate);
                            }

                            //Build totals for tasks:

                            //If this workentry's task is already in the task list, create a new task in the task list. If not, edit total work hours and amount
                            Task thisTask = taskView.Find(x => x.TaskID == workitem.TaskID);
                            if (thisTask == null)
                            {
                                taskView.Add(new Task()
                                {
                                    TaskID = workitem.TaskID,
                                    TaskName = workitem.task.TaskName,
                                    BillableRate = (decimal) workitem.task.BillableRate,
                                    EmpTaskAmt = (workitem.WorkHours * (decimal)workitem.task.BillableRate),
                                    EmpTaskHours = workitem.WorkHours
                                });
                            }
                            else
                            {
                                thisTask.EmpTaskAmt += (workitem.WorkHours * (decimal)workitem.task.BillableRate);
                                thisTask.EmpTaskHours += workitem.WorkHours;
                            }

                        }
                    }

                    List<ExpenseCategory> expCatView = new List<ExpenseCategory>();
                    //Will repeat for every expenseentry for this project
                    foreach (var expitem in item.expenseentries)
                    {
                        if (expitem.EmpID == id)
                        {
                            projectExp += expitem.ExpDollarAmt;

                            //Build totals for cateogories:

                            //If this workentry's category is already in the category list, create a new one. If not, edit total amount
                            ExpenseCategory thisCategory = expCatView.Find(x => x.CategoryName == expitem.ExpCategory);
                            if (thisCategory == null)
                            {
                                expCatView.Add(new ExpenseCategory()
                                {
                                    CategoryName = expitem.ExpCategory,
                                    EmpCatTotalAmt = expitem.ExpDollarAmt,
                                });
                            }
                            else
                            {
                                thisCategory.EmpCatTotalAmt += expitem.ExpDollarAmt;
                            }

                        }
                    }

                    tryIt.Add(new Project()
                    {
                        ProjName = item.ProjName,
                        ProjID = item.ProjID,
                        ContractName = item.contract.ConName,
                        ProjManName = item.employee.EmpFName + " " + item.employee.EmpLName,
                        EmpTotalHr = projectHours,
                        EmpTotalAmt = projectAmt,
                        EmpTotalExp = projectExp,
                        ProjStartDate = item.ProjStartDate,
                        ProjEndDate = item.ProjEndDate,
                        ProjStatus = item.ProjStatus,
                        EmpProjTask = taskView,
                        ProjDesc = item.ProjDesc,
                        EmpProjCategory = expCatView,
                    });
                }
                return tryIt;
            }
        }

    }

}