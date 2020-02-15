using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GrizzTime.BusinessLogic;
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
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Project/Create
        public ActionResult Create()
        {
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
            ViewBag.Message = message;
            ViewBag.Status = Status;

            return View(thisProj);
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int? id)
        {
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
                    ProjID = proj.ProjID,
                    ProjStatus = proj.ProjStatus,
                    ConID = proj.ConID.ToString(),
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
                ViewBag.Message = message;
                ViewBag.Status = Status;
                return RedirectToAction("MyProjects", "Business");
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
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
                    ViewBag.message = message;
                    return RedirectToAction("MyProjects", "Business");
                }
                ViewBag.message = message;
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
                    ViewBag.message = message;
                    return RedirectToAction("MyProjects", "Business");
                }

                dc.projects.Remove(proj);
                dc.SaveChanges();
            }
            ViewBag.message = message;
            return RedirectToAction("MyProjects", "Business");
        }
    }
}
