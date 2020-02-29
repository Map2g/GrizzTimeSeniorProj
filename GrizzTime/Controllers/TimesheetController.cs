using GrizzTime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GrizzTime.ViewModels;
using System.Net;
using System.Data.SqlClient;

namespace GrizzTime.Controllers
{
    public class TimesheetController : Controller
    {
        // GET: Timesheet
        public ActionResult Create()
        {
            string message;

            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists               

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.payrollcycle pc = new GrizzTime.Models.payrollcycle();
                    pc.PayrollCycleStart = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    pc.PayrollCycleEnd = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(7);
                    pc.PayrollCycleYear = (short)System.DateTime.Now.Year;

                    dc.payrollcycles.Add(pc);
                    dc.SaveChanges();

                    GrizzTime.Models.timesheet ts = new GrizzTime.Models.timesheet();
                    ts.PayrollCycleID = pc.PayrollCycleID;
                    ts.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);

                    dc.timesheets.Add(ts);
                    dc.SaveChanges();

                    int tsID = ts.TimeSheetID;


                    message = "New Timesheet and payroll cycle created.";
                    ViewBag.Message = message;
                    return RedirectToAction("Week", "Timesheet", new { id = tsID });
                }
            }
            else
            {
                message = "Something messed up.";
                ViewBag.Message = message;
                return RedirectToAction("Week");
            }
            
        }

        // Edit: Timesheet
        public ActionResult Edit()
        {
            return View();
        }

        // View: All weeks for this employee Timesheet
        [HttpGet]
        public ActionResult List()
        {
            string message = "";
            try
            {
                IEnumerable<timesheet> thisEmployeeTimesheet;

                Entities dc = new Entities();
                
                    int id = Int32.Parse(Request.Cookies["UserID"].Value);
                    thisEmployeeTimesheet = dc.timesheets.Where(x => x.EmpID == id);               

                    if (thisEmployeeTimesheet.Any() == false)
                    {
                        message = "You don't have any timesheets yet!";
                        return View();
                    }
                    else
                    {
                        return View(thisEmployeeTimesheet);
                    }
                
            }
            catch (Exception ex)
            {
                message = "something happened";                      
                return View();
                throw ex;
            }

        }

        // View week entry (edit)
        [HttpGet]
        public ActionResult Week(int? id)
        {
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            ViewBag.TimeSheetID = id.ToString();
            Entities dc = new Entities();
            

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                timesheet ts = dc.timesheets.Find(id);               
                ICollection<workentry> thisWeekTry = ts.workentries;

                if (ts == null)
                {
                    return HttpNotFound();
                }

                return View(thisWeekTry);           
        }

        // View AND edit: Specific timesheet week. id is the id of a timesheet
        //[HttpPost]
        //public ActionResult Week(int? id)
        //{
        //    return View();
        //}

        //tid is timesheet id, wid is workentry id
        [HttpGet]
        public ActionResult WeekEntry(int? tid, int? wid, string DOW)
        {
            var message = "";
            Entities dc = new Entities();

            if (tid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (wid == null)
            {
                if (DOW != null) 
                {
                    workentry thisDay = new workentry()
                    {
                        EmpID = Int32.Parse(Request.Cookies["UserID"].Value),
                        TimeSheetID = (int)tid,
                        WorkDate = DOW,    
                    };

                    dc.workentries.Add(thisDay);

                    try
                    {
                        dc.SaveChanges();
                        return RedirectToAction("WeekEntry", "Timesheet", new { tid, wid = thisDay.WorkEntryID });
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        //more descriptive error for validation problems
                        Exception exception = dbEx;
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                string message1 = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity.ToString(),
                                    validationError.ErrorMessage);

                                //create a new exception inserting the current one
                                //as the InnerException
                                exception = new InvalidOperationException(message1, exception);
                            }
                        }
                        //error for UI
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                        throw exception;
                    }                   
                }
                else
                {
                    message = "Date of week not captured";
                    return View();
                }
            }
            else //wid is not null
            {
                var w = dc.workentries.FirstOrDefault(p => p.WorkEntryID == wid);
                return View(w);
            }
        }


        // View AND edit: Specific timesheet week. id is the id of a timesheet
        [HttpPost]
        public ActionResult WeekEntry(int? tid, int? wid, workentry thisDay)
        {
            string message = "";
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            using (Entities dc = new Entities())
            {

                if (tid == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                timesheet ts = dc.timesheets.Find(tid);

                GrizzTime.Models.workentry we = dc.workentries.FirstOrDefault(p => p.WorkEntryID == wid);
                if (thisDay == null)
                    return HttpNotFound();

                we.WorkHours = thisDay.WorkHours;
                we.ProjID = thisDay.ProjID;
                we.TaskID = thisDay.TaskID;

                dc.Entry(we).State = System.Data.Entity.EntityState.Modified;
                try
                {
                    dc.SaveChanges();
                    message = "Timesheet updated successfully.";
                    ViewBag.Message = message;
                    return RedirectToAction("Week", "Timesheet", new { id = tid });
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    //more descriptive error for validation problems
                    Exception exception = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message1 = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);

                            //create a new exception inserting the current one
                            //as the InnerException
                            exception = new InvalidOperationException(message1, exception);
                        }
                    }
                    //error for UI
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    throw exception;
                }
            }
        }

        // Submit: Timesheet
        public ActionResult Submit()
        {
            return View();
        }
    }
}