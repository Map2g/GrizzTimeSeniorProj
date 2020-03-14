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
                thisEmpTimesheets = dc.timesheets.Where(x => ((x.EmpID == eid) & (x.payrollcycle.PayrollCycleYear.ToString() == year)) ).OrderByDescending(x=> x.payrollcycle.PayrollCycleStart).ToList();

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
                    Supervisor = new Employee() { UserID = emp.employee2.UserID, 
                                                    EmpPhone = emp.employee2.EmpPhone, 
                                                    EmpFName = emp.employee2.EmpFName, 
                                                    EmpLName = emp.employee2.EmpLName, 
                                                    UserEmail = emp.employee2.UserEmail },               
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

        public ActionResult ProjectInvoice(int pid)
        {
            return View();
        }
    }
}