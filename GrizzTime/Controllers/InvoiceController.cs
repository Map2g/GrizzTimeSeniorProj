using GrizzTime.Models;
using GrizzTime.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        [HttpGet]
        public ActionResult EmployeeInvoice(int? eid, string year)
        {
            ViewBag.year = year;
            using (Entities dc = new Entities())
            {

                if (eid == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                //------Get database data before filling in Employee View Model------

                //Employee information
                employee emp = dc.employees.Find(eid);

                //Timesheet information
                List<timesheet> thisEmpTimesheets;

                //Get all this employee's timesheets for the selected year , order by newest first in the list and oldest last in the list.
                thisEmpTimesheets = dc.timesheets.Where(x => ((x.EmpID == eid) & (x.payrollcycle.PayrollCycleYear.ToString() == year))).OrderByDescending(x => x.payrollcycle.PayrollCycleStart).ToList();

                if (thisEmpTimesheets.Any() == false)
                {
                    //This should never occur.
                    TempData["message"] = "How could it have come to this??";
                    return View();
                }

                string latest = thisEmpTimesheets.First().payrollcycle.PayrollCycleStart.ToShortDateString();
                string earliest = thisEmpTimesheets.Last().payrollcycle.PayrollCycleStart.ToShortDateString();

                ViewBag.earliest = earliest;
                ViewBag.latest = latest;

                decimal totalHours = 0;
                decimal totalEarned = 0;

                foreach (var item in thisEmpTimesheets)
                {
                    totalHours += item.TimeSheetTotalHr;
                    totalEarned += item.TimeSheetTotalAmt;
                }

                //------------------------------------------------------------------------
                Employee viewEmpInv = new Employee()
                {
                    EmpFName = emp.EmpFName,
                    EmpLName = emp.EmpLName,
                    EmpType = emp.EmpType,
                    UserEmail = emp.UserEmail,
                    EmpPhone = emp.EmpPhone,
                    UserID = emp.UserID,
                    Supervisor = new Employee()
                    {
                        UserID = emp.employee2.UserID,
                        EmpPhone = emp.employee2.EmpPhone,
                        EmpFName = emp.employee2.EmpFName,
                        EmpLName = emp.employee2.EmpLName,
                        UserEmail = emp.employee2.UserEmail
                    },
                    YearTotalEarned = totalEarned,
                    YearTotalHours = totalHours,
                    //Below contains all the individual totals and data for this employee's projects and tasks
                    EmployeeProjects = Employee.GetProjects(emp.UserID)
                };

                if (emp == null)
                {
                    return HttpNotFound();
                }

                return View(viewEmpInv);
            }
        }

        [HttpGet]
        public ActionResult EmployeeInvoice_Print(int? eid, string year)
        {
            return EmployeeInvoice(eid, year);
        }

        public ActionResult ProjectInvoice(int id)
        {

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            ViewBag.UserID = Request.Cookies["UserID"].Value;
            using (Entities dc = new Entities())
            {

                project proj = dc.projects.Find(id);

                //List<workentry> we;

                List<project> thisProjTimesheets;

                //we = dc.workentries.Where(x => x.ProjID == id).ToList();

                thisProjTimesheets = dc.projects.Where(x => x.ProjID == id).ToList();

                decimal totalProjHours = 0;

                foreach (var item in thisProjTimesheets)
                {
                    totalProjHours += item.ProjTotalHr;
                }

                Project projects = new Project()
                {
                    ProjName = proj.ProjName,
                    ProjDesc = proj.ProjDesc,
                    ProjStartDate = proj.ProjStartDate,
                    ProjEndDate = proj.ProjEndDate,
                    ProjManName = proj.employee.EmpFName + " " + proj.employee.EmpLName,
                    ProjID = proj.ProjID,
                    ProjStatus = proj.ProjStatus,
                    ContractName = proj.contract.ConName,
                    ProjTotalHr = totalProjHours,
                    ProjManID = proj.employee.UserID.ToString(),
                    //EmployeeProjects = Project.GetEmployees(proj.ProjID),
                    Contract = new Contract()
                    {
                        ConID = proj.contract.ConID,
                        ConName = proj.contract.ConName,
                        ConAllottedHours = (decimal)proj.contract.ConAllottedHours,
                        ConHoursRemaining = proj.contract.ConHoursRemaining
                    },

                };
                return View(projects);
            }
        }

    }
}
    //public ActionResult ProjectInvoice(int id)
    //{
    //    if (Request.Cookies["UserID"].Value == null)
    //    {
    //        //Redirect to login if it can't find user id
    //        TempData["message"] = "Please log in.";
    //        System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
    //        return RedirectToAction("LandingPage", "Home");
    //    }

//    ViewBag.UserID = Request.Cookies["UserID"].Value;

//    dynamic model = new ExpandoObject();
//    model.Project = GetProjects(id);
//    //model.WorkEntry = GetWorkEntries(id);
//    return View(model);

//}

//private List<Project> GetProjects(int id)
//{
//    ViewBag.UserID = Request.Cookies["UserID"].Value;
//    using (Entities dc = new Entities())
//    {
//        List<Project> projects = new List<Project>();
//        project proj = dc.projects.Find(id);
//        Project viewProj = new Project();

//        projects.Add(new Project
//        {
//            ProjName = proj.ProjName,
//            ProjDesc = proj.ProjDesc,
//            ProjStartDate = proj.ProjStartDate,
//            ProjEndDate = proj.ProjEndDate,
//            ProjManName = proj.employee.EmpFName + " " + proj.employee.EmpLName,
//            ProjID = proj.ProjID,
//            ProjStatus = proj.ProjStatus,
//            ContractName = proj.contract.ConName,
//            //EmpProjTask = Project.GetTasks(proj.ProjID),


//        });
//        return projects;
//    }
//}


//private List<WorkEntry> GetWorkEntries(int ProjID)
//{

//    ViewBag.UserID = Request.Cookies["UserID"].Value;
//    using (Entities dc = new Entities())
//    {
//        int workEntriesSize;
//        //List<WorkEntry> workEntries = new List<WorkEntry>();
//        List<workentry> workEntries;
//        workEntries = dc.workentries.Where(x => x.ProjID == ProjID).ToList();
//        workentry we = dc.workentries.Find(ProjID);
//        List<WorkEntry> test = new List<WorkEntry>();



//        //if(dc.workentries.Where(x => x.ProjID == ProjID).Any())
//        //{
//            //workEntries.Add(new WorkEntry
//            //{
//            //    ProjID = we.ProjID
//            //});
//        //}
//        //workEntries = dc.workentries.Where(x => x.ProjID == id).
//        //workEntriesSize = workEntries.Count;

//        //for(int i=1; i<workEntries.Count; i++)
//        //{

//        //}



//        return workEntries;
//    }
//}