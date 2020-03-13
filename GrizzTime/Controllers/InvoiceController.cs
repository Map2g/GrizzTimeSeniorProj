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
    public class InvoiceController : Controller
    {
        public ActionResult EmployeeInvoice(int? eid, string year)
        {
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
                thisEmpTimesheets = dc.timesheets.Where(x => ((x.EmpID == eid) & (x.payrollcycle.PayrollCycleYear.ToString() == year)) ).ToList();

                if (thisEmpTimesheets.Any() == false)
                {
                    //This should never occur.
                    TempData["message"] = "How could it have come to this??";
                    return View();
                }

                decimal totalHours = 0;
                decimal totalEarned = 0;

                foreach (var item in thisEmpTimesheets)
                {
                    totalHours = totalHours + item.TimeSheetTotalHr;
                    //totalEarned = totalEarned + item.payrollcycle;
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
                    SupervisorID = emp.SupervisorID,
                    
                };

                if (emp == null)
                {
                    return HttpNotFound();
                }

                return View(viewEmpInv);
            }
        }

        public ActionResult ProjectInvoice(int pid)
        {
            return View();
        }
    }
}