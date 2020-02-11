using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.SqlClient;
using System.Data;
using GrizzTime.Models;
using System.Text;

namespace GrizzTime.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Registration(int? id)
        {
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                employee employee = dc.employees.Find(id);
                if (employee == null)
                {
                    return HttpNotFound();
                }

                return View(employee);
            }
        }


        [HttpPost]
        public ActionResult Registration(employee employee)
        {

            bool Status = false;
            string message = "";
            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    var thisEmp = dc.employees.FirstOrDefault(p => p.UserID == employee.UserID);
                    if (thisEmp == null)
                        return HttpNotFound();

                    thisEmp.UserPW = Hash(employee.UserPW);
                    thisEmp.UserStatus = employee.UserStatus;
                    //dc.Entry(employee).State = System.Data.Entity.EntityState.Modified;
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
                    //send email to employee
                    SendVerificationEMail(thisEmp.UserEmail);
                    message = "Registration complete! An email has been sent to you to confirm your registration!";
                    Status = true;
                }
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View();
        }


        //    //ensure that the model exists
        //    if (ModelState.IsValid)
        //    {

        //        //Save to Database
        //        using (Entities dc = new Entities())
        //        {
        //            dc.Entry(employee).State = System.Data.Entity.EntityState.Modified;
        //            employee.UserPW = Hash(employee.UserPW);
        //            try
        //            {
        //                dc.SaveChanges();
        //            }
        //            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
        //            {
        //                Exception exception = dbEx;
        //                foreach (var validationErrors in dbEx.EntityValidationErrors)
        //                {
        //                    foreach (var validationError in validationErrors.ValidationErrors)
        //                    {
        //                        string message1 = string.Format("{0}:{1}",
        //                            validationErrors.Entry.Entity.ToString(),
        //                            validationError.ErrorMessage);

        //                        //create a new exception inserting the current one
        //                        //as the InnerException
        //                        exception = new InvalidOperationException(message1, exception);
        //                    }
        //                }
        //                throw exception;
        //            }

        //            //send email to employee
        //            SendVerificationEMail(employee.UserEmail);
        //            message = "Registration complete! An email has been sent to you to confirm your registration!";
        //            Status = true;
        //        }
        //    }
        //    else
        //    {
        //        message = "Invalid Request";
        //    }
        //    ViewBag.Message = message;
        //    ViewBag.Status = Status;
        //    return View();
        //}

        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Login(employee employee, String ReturnUrl)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                var v = dc.employees.Where(a => a.UserEmail == employee.UserEmail).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(employee.UserPW, v.UserPW) == 0)
                    {
                        bool LRememberMe = employee.RememberMe;
                        int timeout = LRememberMe ? 52600 : 20; // Remembers for one year
                        var ticket = new FormsAuthenticationTicket(employee.UserEmail, LRememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    message = "Invalid Credentials";
                }
            }

            ViewBag.Message = message;
            return View();
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Employee");
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            Entities db = new Entities();
            return View(from employee in db.employees select employee);
        }

        // GET: User/Edit/5
        public ActionResult Edit(int? id)
        {
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                employee employee = dc.employees.Find(id);
                if (employee == null)
                {
                    return HttpNotFound();
                }

                return View(employee);
            }

        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind(Include = "UserID, UserStatus, RememberMe, BusCode, EmpFName,EmpLName,UserEmail,UserPW, EmpType, EmpPhone")]employee employee)
        {
            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    dc.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        dc.SaveChanges();
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
                    return RedirectToAction("details");
                }
            }
            return RedirectToAction("Index");

        }

        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            using (Entities dc = new Entities())
            {
                employee employee = dc.employees.Find(id);

                if (employee == null)
                    return View("NotFound");

                return View(employee);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string confirmButton)
        {
            using (Entities dc = new Entities())
            {
                employee employee = dc.employees.Find(id);

                if (employee == null)
                    return View("NotFound");

                dc.employees.Remove(employee);
                dc.SaveChanges();
            }
            return View("Deleted");
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (Entities dc = new Entities())
            {
                var v = dc.employees.Where(a => a.UserEmail == emailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationEMail(string email)
        {
            var verifyUrl = "/User/VerifyAccount";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Your account hase been succesfully created!";

            string body = "<br/><br/> We are excited to tell you that you're GrizzTime account has been created!...";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        public static string Hash(string value)
        {
            return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value))
                );
        }
    }

}