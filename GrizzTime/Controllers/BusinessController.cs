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
    public class BusinessController : Controller
    {
        // GET: Business
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            try
            {
                ViewBag.BusinessName = Request.Cookies["BusinessName"].Value;
                ViewBag.BusinessID = Request.Cookies["UserID"].Value;
            }
            catch (NullReferenceException e)
            { //Redirect to login if it can't find business name
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n" + e.Message);
                return RedirectToAction("Login", "Business");
            }

            return View();
        }

        public ActionResult AddEmployeePopUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddEmployeePopUp([Bind(Exclude = "IsEmailVerified,ActivationCode")] employee employee)
        {
            bool Status = false;
            string message = "";
            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists
                var isExist = IsEmailExist(employee.UserEmail);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "An employee with this email address already exists.");
                    return View(employee);
                }

                ////Check validity of business code
                //Entities dc = new Entities();
                //var validBusiness = from business in dc.businesses
                //                    where business.UserID == employee.BusCode
                //                    select business;
                //if (validBusiness.Count() != 1)
                //{
                //    ModelState.AddModelError("InvalidBusinesscode", "That business code does not exist");
                //    return View(employee);
                //}

                Entities dc = new Entities();
                //Save to Database
                using (dc)
                {
                    employee.UserPW = Hash(employee.UserPW);
                    dc.employees.Add(employee);
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


                    //send email to User
                    SendRegistrationEMail(employee.UserEmail, employee.UserID);
                    message = "A link to finish registration was sent to the employee.";
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

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] business business)
        {
            bool Status = false;
            string message = "";
            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists
                var isExist = IsEmailExist(business.UserEmail);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exists");
                    return View(business);
                }
                //Save to Database
                using (Entities dc = new Entities())
                {
                    dc.businesses.Add(business);
                    dc.SaveChanges();

                    //send email to User
                    SendVerificationEMail(business.UserEmail);
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

        public ActionResult Login()
        {

            return View();
        }
        
        [HttpPost]
        public ActionResult Login(business business, String ReturnUrl)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                var v = dc.businesses.Where(a => a.UserEmail == business.UserEmail).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(business.UserPW, v.UserPW) == 0)
                    {
                        
                        int timeout = business.RememberMe ? 52600 : 20; // Remembers for one year
                        var ticket = new FormsAuthenticationTicket(business.UserEmail, business.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        Response.Cookies.Add(new HttpCookie("UserID", v.UserID.ToString() ) );
                        Response.Cookies.Add(new HttpCookie("BusinessName", v.BusName ));



                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return Redirect("Dashboard");
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
            return RedirectToAction("Login", "Business");
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            Entities db = new Entities();
            return View(from business in db.businesses select business);
            //var Details = new List<business>()
            //{
            //    new business()
            //    {
            //        UserID=1
            //    }
            //};
        }

        // GET: User/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            Entities dc = new Entities();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            business business1 = dc.businesses.Find(id);
            if (business1 == null)
            {
                return HttpNotFound();
            }

            return View(business1);

        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID, UserStatus, RememberMe, UserEmail, UserPW, BusName, BusAddress, BusDesc")]business business)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (Entities dc = new Entities())
                    {
                    
                        dc.Entry(business).State = System.Data.Entity.EntityState.Modified;
                        dc.SaveChanges();
                        return RedirectToAction("Details");
                    }
                }
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

            return View(business);

        }

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            using (Entities dc = new Entities())
            {
                business business = dc.businesses.Find(id);

                if (business == null)
                    return View("NotFound");

                return View(business);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string confirmButton)
        {
            using (Entities dc = new Entities())
            {
                business business = dc.businesses.Find(id);

            if (business == null)
                    return View("NotFound");

                dc.businesses.Remove(business);
                dc.SaveChanges();
            }
            return View("Deleted");
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (Entities dc = new Entities())
            {
                var v = dc.businesses.Where(a => a.UserEmail == emailID).FirstOrDefault();
                var t = dc.employees.Where(a => a.UserEmail == emailID).FirstOrDefault();
                return v != null || t != null;
            }
        }

        [NonAction]
        public void SendVerificationEMail(string email)
        {
            var verifyUrl = "/employee/registration/";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Your account hase been succesfully created!";

            string body = "Congratulations, your business account has been created! GrizzTime Senior Project testing.";

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

        [NonAction]
        public void SendRegistrationEMail(string email, int employeeId)
        {
            var verifyUrl = "/employee/registration/" + employeeId.ToString();
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Your account hase been succesfully created!";

            string body = "You have been registered as an employee of " + Request.Cookies["BusinessName"].Value + ". To finish setting up your account, click here: <a href='" + link + "'>link</a>";

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
