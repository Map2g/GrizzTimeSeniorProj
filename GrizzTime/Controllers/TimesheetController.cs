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

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                ViewBag.Message = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists               

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.payrollcycle pc = new GrizzTime.Models.payrollcycle();
                    pc.PayrollCycleStart = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday).Date;
                    pc.PayrollCycleEnd = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(7).Date;
                    pc.PayrollCycleYear = (short)System.DateTime.Now.Year;

                    dc.payrollcycles.Add(pc);
                    dc.SaveChanges();

                    GrizzTime.Models.timesheet ts = new GrizzTime.Models.timesheet();
                    ts.PayrollCycleID = pc.PayrollCycleID;
                    ts.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                    ts.TimeSheetStatus = "In Progress";

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
        public ActionResult List(string m)
        {
            string message = m;

            if (!Request.Cookies.AllKeys.Contains("UserID"))
            {
                //Redirect to login if it can't find user id
                message = "Please log in.";
                ViewBag.Message = message;
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            try
            {
                IEnumerable<timesheet> thisEmployeeTimesheet;
                Entities dc = new Entities();
                
                    int id = Int32.Parse(Request.Cookies["UserID"].Value);
                    thisEmployeeTimesheet = dc.timesheets.Where(x => x.EmpID == id).OrderByDescending(p=>p.payrollcycle.PayrollCycleStart).ToList();               

                    if (thisEmployeeTimesheet.Any() == false)
                    {
                        message = "You don't have any timesheets yet!";
                        ViewBag.Message = message;
                        return View();
                    }
                    else
                    {
                        ViewBag.Message = message;
                        return View(thisEmployeeTimesheet);
                    }
                
            }
            catch (Exception ex)
            {
                message = "something happened";
                ViewBag.Message = message;
                return View();
                throw ex;
            }
            
        }

        // View week entry (edit)
        public ActionResult Week(int? id)
        {
            ViewBag.IsChangeable = true;
            //if (!Response.Cookies.AllKeys.Contains("UserID"))
            //{
            //    //Redirect to login if it can't find user id
            //    ViewBag.Message = "Please log in.";
            //    System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
            //    return RedirectToAction("LandingPage", "Home");
            //}

            ViewBag.UserID = Request.Cookies["UserID"].Value;

            if (id == null)
            {
                return HttpNotFound();
            }

            ViewBag.TimeSheetID = (int) id;

            Entities dc = new Entities();          

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            timesheet ts = dc.timesheets.Find(id);   
            
            //disable edit buttons
            if (ts.TimeSheetStatus != "In Progress")
            {
                bool changeable = false;
                ViewBag.IsChangeable = changeable;
            }

            ICollection<workentry> thisWeekTry = ts.workentries;

            if (ts == null)
            {
                return HttpNotFound();
            }

            return View(thisWeekTry);           
        }

        //tid is timesheet id, wid is workentry id
        [HttpGet]
        public ActionResult WorkEntry(int? tid, int? wid, string DOW)
        {

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                ViewBag.Message = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            var message = "";
            ViewBag.IsExist = false;
            ViewBag.TimeSheetID = (int) tid;
            ViewBag.UserID = Int32.Parse(Request.Cookies["UserID"].Value);

            Entities dc = new Entities();

            if (tid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (wid == null)
            {
                if (DOW != null) 
                {
                    ViewBag.DayOfWeek = DOW;
                    return View();              
                }
                else
                {                                     
                    message = "Date of week not captured.";
                    return HttpNotFound();
                }
            }
            else //wid is not null
            {
                ViewBag.IsExist = true;
                ViewBag.IsChangeable = true;              

                timesheet ts = dc.timesheets.Find(tid);
                if (ts.TimeSheetStatus != "In Progress")
                {
                    bool changeable = false;
                    ViewBag.IsChangeable = changeable;
                }

                var w = dc.workentries.FirstOrDefault(p => p.WorkEntryID == wid);
                WorkEntry thisWE = new WorkEntry()
                {
                    EmpID = w.EmpID,
                    possibleProjects = Employee.GetProjects(w.EmpID),
                    possibleTasks = Project.GetTasks(w.ProjID), 
                    WorkDate = w.WorkDate,
                    WorkHours = w.WorkHours,
                    TimeSheetID = w.TimeSheetID,
                    WorkEntryID = w.WorkEntryID,  
                    ProjID = w.ProjID,
                    TaskID = w.TaskID,                   
                };
                ViewBag.DayOfWeek = thisWE.WorkDate;
                return View(thisWE);
            }
        }


        // View AND edit: Specific timesheet week. id is the id of a timesheet
        [HttpPost]
        public ActionResult WorkEntry(int? tid, int? wid, WorkEntry thisDay)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                ViewBag.Message = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            string message = "";
            ViewBag.IsExist = false;
            ViewBag.TimeSheetID = (int)tid;
            ViewBag.UserID = Int32.Parse(Request.Cookies["UserID"].Value);
            using (Entities dc = new Entities())
            {

                if (tid == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                timesheet ts = dc.timesheets.Find(tid);

                GrizzTime.Models.workentry we = dc.workentries.FirstOrDefault(p => p.WorkEntryID == wid);
                if (we == null) //create new
                {
                    workentry w = new workentry()
                    {
                        EmpID = Int32.Parse(Request.Cookies["UserID"].Value),
                        ProjID = thisDay.ProjID,
                        TaskID = thisDay.TaskID,
                        WorkDate = thisDay.WorkDate,
                        WorkHours = thisDay.WorkHours,
                        TimeSheetID = (int)tid,
                    };
                    dc.workentries.Add(w);

                    WorkEntry thisWE = new WorkEntry()
                    {
                        EmpID = w.EmpID,
                        possibleProjects = Employee.GetProjects(w.EmpID),
                        possibleTasks = Project.GetTasks(w.ProjID),
                        WorkDate = w.WorkDate,
                        WorkHours = w.WorkHours,
                        TimeSheetID = w.TimeSheetID,
                        WorkEntryID = w.WorkEntryID,
                        ProjID = w.ProjID,
                        TaskID = w.TaskID,
                    };

                    //add try catch
                    dc.SaveChanges();

                    ViewBag.Message = "Successfully saved.";
                    //return View(thisWE);
                    return Redirect("Week/" + tid);
                }
                else //edit existing
                {
                    ViewBag.IsExist = true;
                    we.WorkHours = thisDay.WorkHours;
                    we.ProjID = thisDay.ProjID;
                    we.TaskID = thisDay.TaskID;

                    WorkEntry thisWE = new WorkEntry()
                    {
                        EmpID = we.EmpID,
                        possibleProjects = Employee.GetProjects(we.EmpID),
                        possibleTasks = Project.GetTasks(we.ProjID),
                        WorkDate = we.WorkDate,
                        WorkHours = we.WorkHours,
                        TimeSheetID = we.TimeSheetID,
                        WorkEntryID = we.WorkEntryID,
                        ProjID = we.ProjID,
                        TaskID = we.TaskID,
                    };

                    dc.Entry(we).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        dc.SaveChanges();
                        message = "Time updated successfully.";
                        ViewBag.Message = message;

                        //return View(thisWE);
                        return Redirect("Week/"+ tid );
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
        }

        // Submit: Timesheet
        public ActionResult Submit(int? tid)
        {
            Entities dc = new Entities();

            timesheet ts = dc.timesheets.Find(tid);

            if (ts.TimeSheetStatus != "In Progress")
            {
                TempData["message"] = "Timesheet already submitted!";
                return RedirectToAction("List");
            }

            ts.TimeSheetStatus = "Pending";
            ts.TimeSheetSubmitTime = System.DateTime.Now;

            dc.Entry(ts).State = System.Data.Entity.EntityState.Modified;
            try
            {
                dc.SaveChanges();
                TempData["message"] = "Timesheet submitted successfully!";
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

                        //create a new exception inserting the current one as the InnerException
                        exception = new InvalidOperationException(message1, exception);
                    }
                }
                //error for UI
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                throw exception;
            }
        }

        public ActionResult Approve(int? tid)
        {
            Entities dc = new Entities();

            timesheet ts = dc.timesheets.Find(tid);

            if (ts.TimeSheetStatus != "Pending")
            {
                TempData["message"] = "Action was already taken.";
                return RedirectToAction("PendingApprovals");
            }

            ts.TimeSheetStatus = "Approved";
            ts.TimeSheetApproveTime = System.DateTime.Now;

            dc.Entry(ts).State = System.Data.Entity.EntityState.Modified;
            try
            {
                dc.SaveChanges();
                TempData["message"] = "Timesheet approved successfully";
                return Redirect("PendingApprovals");
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

                        //create a new exception inserting the current one as the InnerException
                        exception = new InvalidOperationException(message1, exception);
                    }
                }
                //error for UI
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                throw exception;
            }
        }

        public ActionResult Deny(int? tid)
        {
            Entities dc = new Entities();

            timesheet ts = dc.timesheets.Find(tid);

            if (ts.TimeSheetStatus != "Pending")
            {
                TempData["message"] = "Action already taken.";
                return RedirectToAction("PendingApprovals");
            }

            ts.TimeSheetStatus = "Denied";
            ts.TimeSheetApproveTime = System.DateTime.Now; //maybe change this column?

            dc.Entry(ts).State = System.Data.Entity.EntityState.Modified;
            try
            {
                dc.SaveChanges();
                TempData["message"] = "Timesheet denied successfully.";
                return Redirect("PendingApprovals");
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

                        //create a new exception inserting the current one as the InnerException
                        exception = new InvalidOperationException(message1, exception);
                    }
                }
                //error for UI
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                throw exception;
            }
        }

        [HttpGet]
        public ActionResult PendingApprovals()
        {
            string message = "";

            if (!Request.Cookies.AllKeys.Contains("UserID"))
            {
                //Redirect to login if it can't find user id
                message = "Please log in.";
                TempData["message"] = message;
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            try
            {
                IEnumerable<timesheet> pendingApprovals;
                Entities dc = new Entities();

                int id = Int32.Parse(Request.Cookies["UserID"].Value);
                //employee.employee2 is submitting employee's supervisor
                pendingApprovals = dc.timesheets.Where(x => x.employee.employee2.UserID == id && x.TimeSheetStatus == "Pending").OrderByDescending(p => p.payrollcycle.PayrollCycleStart).ToList();

                if (pendingApprovals.Any() == false)
                {
                    message = "No timesheets to approve.";
                    TempData["message"] = message;
                    return View();
                }
                else
                {
                    return View(pendingApprovals);
                }

            }
            catch (Exception ex)
            {
                message = "something happened";
                TempData["message"] = message;
                return View();
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadTasksForProject(string projId)
        {
            int pID = Int32.Parse(projId);
            List<SelectListItem> taskList = new List<SelectListItem>();

            List<task> tasklist_l = Project.GetTasks(pID);

            //convert task list to a select list
            taskList = tasklist_l.ConvertAll(a =>
            {
                return new SelectListItem()
                {
                    Text = a.TaskName,
                    Value = a.TaskID.ToString()
                };
            });

            return Json(taskList, JsonRequestBehavior.AllowGet);
        }
    }
}