using GrizzTime.ViewModels;
using GrizzTime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.Controllers
{
    public class ContractController : Controller
    {
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Contract thisCon)
        {
            bool Status = false;
            string message;

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            //ensure that the model exists
            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.contract con = new GrizzTime.Models.contract();
                    con.ConName = thisCon.ConName;
                    con.ConAllottedHours = thisCon.ConAllottedHours;
                    con.ConHoursRemaining = thisCon.ConAllottedHours;
                    con.BusID = Int32.Parse(Request.Cookies["UserID"].Value);

                    dc.contracts.Add(con);

                    try
                    {
                        dc.SaveChanges();
                        message = "Contract was successfully created.";
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

                Status = true;
            }
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            ViewBag.Status = Status;

            return View(thisCon);
        }

        // GET: Contract/Details/5
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

                contract con = dc.contracts.Find(id);
                Contract viewCon = new Contract()
                {
                    ConName = con.ConName,
                    ConAllottedHours = con.ConAllottedHours,
                    ConHoursRemaining = con.ConHoursRemaining,
                };

                if (con == null)
                {
                    return HttpNotFound();
                }

                return View(viewCon);
            }
        }


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

                contract con = dc.contracts.Find(id);
                Contract viewCon = new Contract()
                {
                    ConName = con.ConName,
                    ConAllottedHours = con.ConAllottedHours,
                    ConHoursRemaining = con.ConHoursRemaining,
                };

                if (con == null)
                {
                    return HttpNotFound();
                }

                return View(viewCon);
            }
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(int? id, Contract thisCon)
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
                    GrizzTime.Models.contract con = dc.contracts.FirstOrDefault(p => p.ConID == id);
                    if (thisCon == null)
                        return HttpNotFound();

                    con.ConName = thisCon.ConName;

                    var oldHoursRemaining = con.ConHoursRemaining;
                    con.ConHoursRemaining = thisCon.ConAllottedHours - (con.ConAllottedHours - oldHoursRemaining);

                    con.ConAllottedHours = thisCon.ConAllottedHours;

                    dc.Entry(con).State = System.Data.Entity.EntityState.Modified;
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
                   
                    Status = true;
                }
                message = "Contract updated successfully.";
            }
            else
            {
                message = "Invalid Request";
            }
            
            TempData["message"] = message;
            ViewBag.Status = Status;
            return RedirectToAction("MyContracts", "Business");
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                contract con = dc.contracts.Find(id);

                if (con == null)
                {
                    message = "Contract not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyContracts", "Business");
                }

                TempData["message"] = message;
                return View(con);
            }
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, string confirmButton)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                contract con = dc.contracts.Find(id);

                if (con == null)
                {
                    message = "Contract not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyContracts", "Business");
                }

                dc.contracts.Remove(con);
                dc.SaveChanges();
            }

            TempData["message"] = message;
            return RedirectToAction("MyContracts", "Business");
        }
    }
}
