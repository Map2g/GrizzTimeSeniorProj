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
            Entities dc = new Entities();
            

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                timesheet ts = dc.timesheets.Find(id);               
                ICollection<workentry> thisWeekTry = ts.workentries;

                //if (thisWeekTry == null)
                //{
                //    //string message = "";
                //    ViewBag.UserID = Request.Cookies["UserID"].Value;                                  

                //        workentry mondayWE = new workentry();
                //        mondayWE.TimeSheetID = ts.TimeSheetID;
                //        mondayWE.WorkDate = DayOfWeek.Monday.ToString();
                //        mondayWE.WorkHours = 0;
                //        mondayWE.ProjID = null;
                //        mondayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        mondayWE.TaskID = null;

                //        workentry tuesdayWE = new workentry();
                //        tuesdayWE.TimeSheetID = ts.TimeSheetID;
                //        tuesdayWE.WorkDate = DayOfWeek.Tuesday.ToString();
                //        tuesdayWE.WorkHours = 0;
                //        tuesdayWE.ProjID = thisWeek[1].ProjID;
                //        tuesdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        tuesdayWE.TaskID = thisWeek[1].TaskID;

                //        workentry wednesdayWE = new workentry();
                //        wednesdayWE.TimeSheetID = ts.TimeSheetID;
                //        wednesdayWE.WorkDate = DayOfWeek.Wednesday.ToString();
                //        wednesdayWE.WorkHours = thisWeek[2].WorkHours;
                //        wednesdayWE.ProjID = thisWeek[2].ProjID;
                //        wednesdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        wednesdayWE.TaskID = thisWeek[2].TaskID;

                //        workentry thursdayWE = new workentry();
                //        thursdayWE.TimeSheetID = ts.TimeSheetID;
                //        thursdayWE.WorkDate = DayOfWeek.Thursday.ToString();
                //        thursdayWE.WorkHours = thisWeek[3].WorkHours;
                //        thursdayWE.ProjID = thisWeek[3].ProjID;
                //        thursdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        thursdayWE.TaskID = thisWeek[3].TaskID;

                //        workentry fridayWE = new workentry();
                //        fridayWE.TimeSheetID = ts.TimeSheetID;
                //        fridayWE.WorkDate = DayOfWeek.Friday.ToString();
                //        fridayWE.WorkHours = thisWeek[4].WorkHours;
                //        fridayWE.ProjID = thisWeek[4].ProjID;
                //        fridayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        fridayWE.TaskID = thisWeek[4].TaskID;

                //        workentry saturdayWE = new workentry();
                //        saturdayWE.TimeSheetID = ts.TimeSheetID;
                //        saturdayWE.WorkDate = DayOfWeek.Saturday.ToString();
                //        saturdayWE.WorkHours = thisWeek[5].WorkHours;
                //        saturdayWE.ProjID = thisWeek[5].ProjID;
                //        saturdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        saturdayWE.TaskID = thisWeek[5].TaskID;

                //        workentry sundayWE = new workentry();
                //        sundayWE.TimeSheetID = ts.TimeSheetID;
                //        sundayWE.WorkDate = DayOfWeek.Sunday.ToString();
                //        sundayWE.WorkHours = thisWeek[6].WorkHours;
                //        sundayWE.ProjID = thisWeek[6].ProjID;
                //        sundayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                //        sundayWE.TaskID = thisWeek[6].TaskID;

                //        dc.workentries.Add(mondayWE);
                //        dc.workentries.Add(tuesdayWE);
                //        dc.workentries.Add(wednesdayWE);
                //        dc.workentries.Add(thursdayWE);
                //        dc.workentries.Add(fridayWE);
                //        dc.workentries.Add(saturdayWE);
                //        dc.workentries.Add(sundayWE);


                //        try
                //        {
                //            dc.SaveChanges();
                //            message = "Timesheet updated successfully.";
                //            ViewBag.Message = message;
                //            return Redirect("List");
                //        }
                //        catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                //        {
                //            //more descriptive error for validation problems
                //            Exception exception = dbEx;
                //            foreach (var validationErrors in dbEx.EntityValidationErrors)
                //            {
                //                foreach (var validationError in validationErrors.ValidationErrors)
                //                {
                //                    string message1 = string.Format("{0}:{1}",
                //                        validationErrors.Entry.Entity.ToString(),
                //                        validationError.ErrorMessage);

                //                    //create a new exception inserting the current one
                //                    //as the InnerException
                //                    exception = new InvalidOperationException(message1, exception);
                //                }
                //            }
                //            //error for UI
                //            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                //            throw exception;

                //        }
                //    }


                if (ts == null)
                {
                    return HttpNotFound();
                }

                return View(thisWeekTry);           

        }

        // View AND edit: Specific timesheet week. id is the id of a timesheet
        [HttpPost]
        public ActionResult Week(int? id, workentry[] thisWeek)
        {
            string message = "";
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                timesheet ts = dc.timesheets.Find(id);

                //TO DO: Fix below: I will not want to create seven entries for every TS if they didn't work seven days a week.
                //I want to create entries only for the days of the week that they worked.
                workentry mondayWE = new workentry();
                mondayWE.TimeSheetID = ts.TimeSheetID;
                mondayWE.WorkDate = DayOfWeek.Monday.ToString();
                mondayWE.WorkHours = thisWeek[0].WorkHours;
                mondayWE.ProjID = thisWeek[0].ProjID;
                mondayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                mondayWE.TaskID = thisWeek[0].TaskID;

                workentry tuesdayWE = new workentry();
                tuesdayWE.TimeSheetID = ts.TimeSheetID;
                tuesdayWE.WorkDate = DayOfWeek.Tuesday.ToString();
                tuesdayWE.WorkHours = thisWeek[1].WorkHours;
                tuesdayWE.ProjID = thisWeek[1].ProjID;
                tuesdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                tuesdayWE.TaskID = thisWeek[1].TaskID;

                workentry wednesdayWE = new workentry();
                wednesdayWE.TimeSheetID = ts.TimeSheetID;
                wednesdayWE.WorkDate = DayOfWeek.Wednesday.ToString();
                wednesdayWE.WorkHours = thisWeek[2].WorkHours;
                wednesdayWE.ProjID = thisWeek[2].ProjID;
                wednesdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                wednesdayWE.TaskID = thisWeek[2].TaskID;

                workentry thursdayWE = new workentry();
                thursdayWE.TimeSheetID = ts.TimeSheetID;
                thursdayWE.WorkDate = DayOfWeek.Thursday.ToString();
                thursdayWE.WorkHours = thisWeek[3].WorkHours;
                thursdayWE.ProjID = thisWeek[3].ProjID;
                thursdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                thursdayWE.TaskID = thisWeek[3].TaskID;

                workentry fridayWE = new workentry();
                fridayWE.TimeSheetID = ts.TimeSheetID;
                fridayWE.WorkDate = DayOfWeek.Friday.ToString();
                fridayWE.WorkHours = thisWeek[4].WorkHours;
                fridayWE.ProjID = thisWeek[4].ProjID;
                fridayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                fridayWE.TaskID = thisWeek[4].TaskID;

                workentry saturdayWE = new workentry();
                saturdayWE.TimeSheetID = ts.TimeSheetID;
                saturdayWE.WorkDate = DayOfWeek.Saturday.ToString();
                saturdayWE.WorkHours = thisWeek[5].WorkHours;
                saturdayWE.ProjID = thisWeek[5].ProjID;
                saturdayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                saturdayWE.TaskID = thisWeek[5].TaskID;

                workentry sundayWE = new workentry();
                sundayWE.TimeSheetID = ts.TimeSheetID;
                sundayWE.WorkDate = DayOfWeek.Sunday.ToString();
                sundayWE.WorkHours = thisWeek[6].WorkHours;
                sundayWE.ProjID = thisWeek[6].ProjID;
                sundayWE.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                sundayWE.TaskID = thisWeek[6].TaskID;

                dc.workentries.Add(mondayWE);
                dc.workentries.Add(tuesdayWE);
                dc.workentries.Add(wednesdayWE);
                dc.workentries.Add(thursdayWE);
                dc.workentries.Add(fridayWE);
                dc.workentries.Add(saturdayWE);
                dc.workentries.Add(sundayWE);


                try
                {
                    dc.SaveChanges();
                    message = "Timesheet updated successfully.";
                    ViewBag.Message = message;
                    return Redirect("List");
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

                if (ts == null)
                {
                    return HttpNotFound();
                }

                return View(thisWeek);
            }

        }

        // Submit: Timesheet
        public ActionResult Submit()
        {
            return View();
        }
    }
}