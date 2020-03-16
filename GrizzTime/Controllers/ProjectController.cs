using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GrizzTime.ViewModels;
using GrizzTime.Models;

namespace GrizzTime.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult Index()
        {
            return View();
        }

        // GET: Project/Details/5
        public ActionResult Details(int? id)
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

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                project proj = dc.projects.Find(id);

                decimal totalHours = 0;
                decimal totalCost = 0;

                //Will repeat for every workentry for this project
                foreach (var item in proj.workentries1)
                {
                    totalHours += item.WorkHours;
                    if (item.task.IsBillable == true)
                    {
                        totalCost += (item.WorkHours * (decimal)item.task.BillableRate);
                    }
                }

                Project viewProj = new Project()
                {
                    ProjName = proj.ProjName,
                    ProjDesc = proj.ProjDesc,
                    ProjStartDate = proj.ProjStartDate,
                    ProjEndDate = proj.ProjEndDate,
                    ProjManName = proj.employee.EmpFName + " " + proj.employee.EmpLName,
                    ProjID = proj.ProjID,
                    ProjStatus = proj.ProjStatus,
                    //obsolete now but i'm keeping it in case
                    ContractName = proj.contract.ConName,
                    ProjManID = proj.employee.UserID.ToString(),
                    BusID = proj.contract.BusID,
                    Contract = new Contract() { 
                                                ConID = proj.contract.ConID,
                                                ConName = proj.contract.ConName,
                                                ConAllottedHours = (decimal) proj.contract.ConAllottedHours,
                                                ConHoursRemaining = proj.contract.ConHoursRemaining
                                              },
                    ProjTotalCost = totalCost,
                    ProjTotalHr = totalHours,
                };

                if (proj == null)
                {
                    return HttpNotFound();
                }

                return View(viewProj);
            }
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            //Can be a business user or a employee user
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(Project thisProj)
        {
            bool Status = false;
            string message = "";

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            //Can be a business user or a employee user
            ViewBag.UserID = Request.Cookies["UserID"].Value;

            //ensure that the model exists
            if (ModelState.IsValid)
            {

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.project proj = new GrizzTime.Models.project();
                    proj.ProjName = thisProj.ProjName;
                    proj.ProjDesc = thisProj.ProjDesc;
                    proj.ProjStartDate = thisProj.ProjStartDate;
                    proj.ProjManID = Int32.Parse(thisProj.ProjManID);
                    proj.ConID = Int32.Parse(thisProj.ConID);

                    proj.ProjStatus = "Ongoing";

                    dc.projects.Add(proj);

                    try
                    {
                        dc.SaveChanges();
                        message = "Project was successfully created.";
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
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
                        throw exception;
                    }

                }

            }
            else
            {
                message = "Invalid Request";
            }
            TempData["message"] = message;
            ViewBag.Status = Status;

            return View(thisProj);
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int? id)
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

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                project proj = dc.projects.Find(id);
                Project viewProj = new Project()
                {
                    ProjName = proj.ProjName,
                    ProjDesc = proj.ProjDesc,
                    ProjStartDate = proj.ProjStartDate,
                    ProjEndDate = proj.ProjEndDate,
                    ProjManID = proj.ProjManID.ToString(),
                    ProjManName = proj.employee.EmpFName + proj.employee.EmpLName,
                    ProjID = proj.ProjID,
                    ProjStatus = proj.ProjStatus,
                    ConID = proj.ConID.ToString(),
                    ContractName = proj.contract.ConName,
                    IsEnded = proj.ProjStatus.Equals("Ended"),
                };

                if (proj == null)
                {
                    return HttpNotFound();
                }

                return View(viewProj);
            }
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(int? id, Project thisProj)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            ViewBag.UserID = Request.Cookies["UserID"].Value;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            bool Status = false;
            string message = "";

            //Don't check include in validation check
            //ModelState.Remove("UserEmail");
            //ModelState.Remove("EmpFName");
            //ModelState.Remove("EmpLName");
            //ModelState.Remove("EmpPhone");
            //ModelState.Remove("EmpType");


            if (ModelState.IsValid)
            {

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.project proj = dc.projects.FirstOrDefault(p => p.ProjID == id);
                    if (thisProj == null)
                        return HttpNotFound();

                    proj.ProjName = thisProj.ProjName;
                    proj.ProjDesc = thisProj.ProjDesc;
                    proj.ProjStartDate = thisProj.ProjStartDate;
                    proj.ProjManID = Int32.Parse(thisProj.ProjManID);
                    proj.ConID = Int32.Parse(thisProj.ConID);
                    if (thisProj.IsEnded)
                    {
                        proj.ProjStatus = "Ended";
                        if (thisProj.ProjEndDate == null)
                            proj.ProjEndDate = DateTime.Now;
                    }
                    else
                    {
                        proj.ProjStatus = "Active";
                    }

                    dc.Entry(proj).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        dc.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
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
                        throw exception;
                    }
                }
                message = "Project updated successfully.";
                Status = true;
                TempData["message"] = message;
                ViewBag.Status = Status;
                return RedirectToAction("MyProjects", "home");
            }
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            ViewBag.Status = Status;
            return View(thisProj);
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);

                if (proj == null)
                {
                    message = "Project not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyProjects", "Home");
                }
                TempData["message"] = message;
                return View(proj);
            }
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, string confirmButton)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);

                if (proj == null)
                {
                    message = "Project not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyProjects", "Home");
                }

                dc.projects.Remove(proj);
                dc.SaveChanges();
            }
            TempData["message"] = message;
            return RedirectToAction("MyProjects", "Home");
        }

        public ActionResult AddTask(int id)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            //BusID
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            //string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);
                ViewBag.ProjectName = proj.ProjName;
                ViewBag.ProjectID = proj.ProjID;
            }
            //TempData["message"] = message;
            return View();
        }

        [HttpPost]
        public ActionResult AddTask(int id, task addTask)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            ViewBag.UserID = Request.Cookies["UserID"].Value;
            string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);
                ViewBag.ProjectName = proj.ProjName;
                ViewBag.ProjectID = proj.ProjID;
                if (proj == null)
                {
                    message = "Project not found.";
                    TempData["message"] = message;
                    //TODO: Redirect to somewhere more general.
                    return RedirectToAction("MyProjects", "Home");
                }

                task newTask = new task()
                {
                    ProjID = proj.ProjID,
                    TaskName = addTask.TaskName,
                    BillableRate = addTask.BillableRate,
                    IsBillable = true,                  
                };

                dc.tasks.Add(newTask);

                try
                {
                    dc.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
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
                    throw exception;
                }
            }
            message = "Task added successfully.";
            TempData["message"] = message;
            return View();
        }

        public ActionResult EditTask(int? id)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Entities dc = new Entities();
            task task = dc.tasks.Find(id);
            Task viewTask = new Task()
            {
                TaskName = task.TaskName,
                BillableRate = (decimal) task.BillableRate,
                TaskID = task.TaskID,
                BelongsToProject = new Project() {
                                                    ProjName = task.project.ProjName,
                                                    ProjID = task.project.ProjID,
                                                    //could add more later
                                                 },
            };

            if (task == null)
            {
                return HttpNotFound();
            }

            return View(viewTask);

            ////BusID
            //ViewBag.UserID = Request.Cookies["UserID"].Value;
            ////string message = "";
            //using (Entities dc = new Entities())
            //{
            //    project proj = dc.projects.Find(id);
            //    ViewBag.ProjectName = proj.ProjName;
            //    ViewBag.ProjectID = proj.ProjID;
            //}
            ////TempData["message"] = message;
            //return View();
        }

        [HttpPost]
        public ActionResult EditTask(int id, Task thisTask)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            string message;
            ModelState.Remove("TaskName");
            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.task task = dc.tasks.FirstOrDefault(p => p.TaskID == id);
                    if (thisTask == null)
                        return HttpNotFound();

                    task.BillableRate = thisTask.BillableRate;

                    dc.Entry(task).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        dc.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
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
                        throw exception;
                    }
                }

                message = "Project updated successfully.";
                TempData["message"] = message;
                return RedirectToAction("MyProjects", "Home");
            }
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            return View(thisTask);
        }

        public ActionResult MarkEnded(int id, Project thisProj)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);

                if (proj == null)
                {
                    message = "Project not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyProjects", "Home");
                }

                proj.ProjStatus = "Ended";
                proj.ProjEndDate = thisProj.ProjEndDate;

                dc.Entry(proj).State = System.Data.Entity.EntityState.Modified;
                dc.SaveChanges();
            }
            TempData["message"] = message;
            return RedirectToAction("MyProjects", "Home");
        }

        public ActionResult AddEmpToProject(int id)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            //Business ID
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            //string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);
                ViewBag.ProjectName = proj.ProjName;
                ViewBag.ProjectID = proj.ProjID;
                ViewBag.ProjectManager = proj.ProjManID; 
            }
            //TempData["message"] = message;
            return View();
        }

        [HttpPost]
        public ActionResult AddEmpToProject(int id, Employee addEmp)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            ViewBag.UserID = Request.Cookies["UserID"].Value;
            string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(id);
                ViewBag.ProjectName = proj.ProjName;
                ViewBag.ProjectID = proj.ProjID;
                ViewBag.ProjectManager = proj.ProjManID;
                if (proj == null)
                {
                    message = "Project not found.";
                    TempData["message"] = message;
                    //TODO: Redirect to somewhere more general.
                    return RedirectToAction("MyProjects", "Home");
                }

                employee_project empproj = new employee_project()
                {
                    EmpID = addEmp.UserID,
                    ProjID = id,
                    StartDate = DateTime.Now,
                };

                dc.employee_project.Add(empproj);             
                
                try
                {
                    dc.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
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
                    throw exception;
                }
            }
            message = "Employee added successfully.";
            TempData["message"] = message;
            return View();
        }

        public ActionResult RemoveEmpFromProject(int pid, int eid)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                project proj = dc.projects.Find(pid);

                //Get employee_project row where employeeID and projectID match the parametes
                employee_project empproj = dc.employee_project.Where(m => m.EmpID == eid && m.ProjID == pid).SingleOrDefault();

                dc.employee_project.Remove(empproj);

                try
                {
                    dc.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
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
                    throw exception;
                }
            }
            message = "Employee removed successfully.";
            TempData["message"] = message;
            return RedirectToAction("Details", new {id = pid });
        }
    }
}

