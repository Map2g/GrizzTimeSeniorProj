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
using System.Text;
using GrizzTime.Models;
using GrizzTime.ViewModels;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System.IO;

namespace GrizzTime.Controllers
{
    [Authorize]
    public class BusinessController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(business business, String ReturnUrl)
        {      
            using (Entities dc = new Entities())
            {
                var v = dc.businesses.Where(a => a.UserEmail == business.UserEmail).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Hash(business.UserPW), v.UserPW) == 0)
                    {
                        var claims = new List<Claim>();

                        try
                        {
                            claims.Add(new Claim(ClaimTypes.Name, v.UserEmail));
                            var claimIdentities = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                            var ctx = Request.GetOwinContext();
                            var authenticationManager = ctx.Authentication;

                            authenticationManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties() { IsPersistent = false }, claimIdentities);

                            //older start
                            int timeout = business.RememberMe ? 52600 : 20; // Remembers for one year
                            var ticket = new FormsAuthenticationTicket(business.UserEmail, business.RememberMe, timeout);
                            string encrypted = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                            cookie.Expires = DateTime.Now.AddMinutes(timeout);
                            cookie.HttpOnly = true;
                            Response.Cookies.Add(cookie);


                            Response.Cookies.Add(new HttpCookie("UserID", v.UserID.ToString()));
                            Response.Cookies.Add(new HttpCookie("Role", "Business"));
                            Response.Cookies.Add(new HttpCookie("BusinessName", v.BusName));
                            //Response.Cookies.Add(new HttpCookie("BusinessName", v.BusName ));
                            //older end
                        }
                        catch (Exception ex)
                        {
                            TempData["message"] = "Something went wrong.";
                            throw ex;
                        }

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            //return Redirect(ReturnUrl);
                            return RedirectToAction("Dashboard");
                        }
                        else
                        {
                            return RedirectToAction("Dashboard");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("BadCredentials", "Incorrect password.");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("NotExist", "There is no business account associated with this email address.");
                    return View();
                }
            }
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string UserEmail)
        {

            //Email already exists
            var isExist = IsEmailExist(UserEmail);
            if (isExist)
            {
                using (Entities dc = new Entities())
                {
                    var bus = dc.businesses.Where(a => a.UserEmail == UserEmail).FirstOrDefault();

                    if (bus == null)
                    {
                        TempData["message"] = "Something went wrong with SQL query.";
                        return View();
                    }

                    ForgotPasswordEmail(UserEmail, bus.UserID);
                    TempData["message"] = "An email was sent to " + UserEmail + ". Please check your email.";
                }
            }
            else
            {
                TempData["message"] = "There is no account registered with that email address!";
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(int? id)
        {
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                business business = dc.businesses.Find(id);
                Business viewBus = new Business()
                {
                    UserEmail = business.UserEmail,
                    BusName = business.BusName,
                };

                if (business == null)
                {
                    return HttpNotFound();
                }

                return View(viewBus);
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(Business thisBus, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Don't check include in validation check
            ModelState.Remove("UserEmail");
            ModelState.Remove("BusName");
            ModelState.Remove("BusDesc");
            ModelState.Remove("BusAddress");

            if (ModelState.IsValid)
            {

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.business bus = dc.businesses.FirstOrDefault(p => p.UserID == id);
                    if (thisBus == null)
                        return HttpNotFound();

                    bus.UserPW = Hash(thisBus.UserPW);

                    dc.Entry(bus).State = System.Data.Entity.EntityState.Modified;
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
                TempData["message"] = "Success! Please log in.";
                return RedirectToAction("Login", "Business");
            }
            else
            {
                TempData["message"] = "Couldn't complete request.";
            }

            //SendVerificationEMail(thisEmp.UserEmail);
            return View(thisBus);
        }

        public ActionResult Dashboard()
        {
            try
            {
                Entities dc = new Entities();
                int id = Int32.Parse(Request.Cookies["UserID"].Value);
                var v = dc.businesses.Where(a => a.UserID == id).FirstOrDefault();
                ViewBag.BusinessName = v.BusName;

                ViewBag.BusinessID = v.UserID;
            }
            catch (NullReferenceException e)
            { //Redirect to login if it can't find business name
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n" + e.Message);
                return RedirectToAction("Login", "Business");
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] Business thisBus)
        {
            //Check if email exists
            var isExist = IsEmailExist(thisBus.UserEmail);
            if (isExist)
            {
                ModelState.AddModelError("EmailExist", "There is already an account registered with this email address.");
                return View(thisBus);
            }

            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Save to Database
                try
                {
                    using (Entities dc = new Entities())
                    {
                        GrizzTime.Models.business bus = new GrizzTime.Models.business();
                        bus.UserEmail = thisBus.UserEmail;
                        bus.UserPW = Hash(thisBus.UserPW);
                        bus.BusName = thisBus.BusName;
                        bus.BusDesc = thisBus.BusDesc;
                        bus.BusAddress = thisBus.BusAddress;
                        
                        bus.UserStatus = "Registered";

                        Response.Cookies.Add(new HttpCookie("UserID", bus.UserID.ToString()));
                        Response.Cookies.Add(new HttpCookie("Role", "Business"));

                        dc.businesses.Add(bus);
                        dc.SaveChanges();
                    }

                    SendVerificationEMail(thisBus.UserEmail);
                    TempData["message"] = "Registration complete! An email has been sent to you to confirm your registration!";                   

                    return RedirectToAction("Dashboard");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(thisBus);
                }
                //using (Entities dc = new Entities())
                //{
                //    dc.businesses.Add(business);
                //    dc.SaveChanges();

                //    send email to User
                //}
            }
            else
            {
                TempData["message"] = "Invalid Request";
                return View(thisBus);
            }                 
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            string message;
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                business bus = dc.businesses.Find(id);

                if (bus == null)
                {
                    message = "Business not find with id: " + id.ToString();
                    TempData["message"] = message;
                    RedirectToAction("Dashboard");
                }

                List<Contract> temp = new List<Contract>();
                foreach (contract item in bus.contracts)
                {
                    temp.Add(new Contract()
                    {
                        ConName = item.ConName,
                        ConAllottedHours = (decimal)item.ConAllottedHours,
                        ConHoursRemaining = item.ConHoursRemaining,
                        ConID = item.ConID
                    });
                }

                Business viewBus = new Business()
                {
                    UserID = bus.UserID,
                    BusName = bus.BusName,
                    BusAddress = bus.BusAddress,
                    BusDesc = bus.BusDesc,
                    UserEmail = bus.UserEmail,
                    UserPW = bus.UserPW,
                    UserStatus = bus.UserStatus,
                    BusContracts = temp
                };

                return View(viewBus);
            }
        }

        //Administration view
        [AllowAnonymous]
        public ActionResult AllBusinesses()
        {
            Entities db = new Entities();
            return View(from business in db.businesses select business);
        }

        // GET: User/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {

            ViewBag.UserID = Request.Cookies["UserID"].Value;
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                business bus = dc.businesses.Find(id);
                Business viewBus = new Business()
                {
                    BusName = bus.BusName,
                    BusAddress = bus.BusAddress,
                    BusDesc = bus.BusDesc,
                    UserEmail = bus.UserEmail,
                    UserPW = bus.UserPW,
                    UserStatus = bus.UserStatus,
                    UserID = bus.UserID,
                };

                if (bus == null)
                {
                    return HttpNotFound();
                }

                return View(viewBus);
            }

        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, Business thisBus)
        {
            ViewBag.UserID = Request.Cookies["UserID"].Value;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            bool Status = false;
            string message;

            //Don't check include in validation check
            //ModelState.Remove("UserEmail");
            //ModelState.Remove("EmpFName");
            //ModelState.Remove("EmpLName");
            //ModelState.Remove("EmpPhone");
            //ModelState.Remove("EmpType");
            ModelState.Remove("UserPW");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.business bus = dc.businesses.FirstOrDefault(p => p.UserID == id);
                    if (thisBus == null)
                        return HttpNotFound();

                    bus.BusName = thisBus.BusName;
                    bus.BusAddress = thisBus.BusAddress;
                    bus.BusDesc = thisBus.BusDesc;
                    bus.UserEmail = thisBus.UserEmail;
                    bus.UserStatus = thisBus.UserStatus;

                    dc.Entry(bus).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        dc.SaveChanges();
                        message = "Business updated successfully.";
                        Status = true;
                        TempData["message"] = message;
                        ViewBag.Status = Status;
                        return RedirectToAction("Dashboard");
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
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            ViewBag.Status = Status;
            return View(thisBus);
        }

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            using (Entities dc = new Entities())
            {
                business business = dc.businesses.Find(id);

                if (business == null)
                {
                    TempData["message"] = "Business does not exist.";
                    return View("Details");
                }
                return View(business);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string confirmButton)
        {
            string message;
            using (Entities dc = new Entities())
            {
                business business = dc.businesses.Find(id);

                if (business == null)
                {
                    message = "Business not found.";
                    TempData["message"] = message;
                    return RedirectToAction("Details");
                }

                dc.businesses.Remove(business);
                dc.SaveChanges();
                message = "Business successfully deleted.";
            }
            TempData["message"] = message;
            return View("Details");
        }

        public ActionResult MyContracts()
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            var id = Request.Cookies["UserID"].Value;
            string query = "SELECT * FROM grizztime.contract WHERE BusID = @id";
            Entities dc = new Entities();

            IEnumerable<contract> thisBusinessContract = dc.contracts.SqlQuery(query, new SqlParameter("@id", id));
            return View(thisBusinessContract);
        }

        public ActionResult MyEmployees()
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            var id = Request.Cookies["UserID"].Value;
            //string query = "SELECT * FROM grizztime.employee WHERE BusCode = @id";
            //Entities dc = new Entities();

            //IEnumerable<employee> thisBusinessEmployee = dc.employees.SqlQuery(query, new SqlParameter("@id", id));
            List<Employee> thisBusinessEmployee = Employee.EmployeeList(id);

            return View(thisBusinessEmployee);
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

        //Does not send link?
        [NonAction]
        public void SendVerificationEMail(string email)
        {
            //var verifyUrl = "/employee/registration/";
            //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Your account has been succesfully created!";

            string body = "Congratulations, your business account has been created!";

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
            var completeRegister = Url.Action("ResetPassword", "Employee", new { id = employeeId});/*"/employee/registration/" + employeeId.ToString();*/
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, completeRegister);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com", "GrizzTime Do Not Reply");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Your account has been succesfully created!";

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

        [NonAction]
        public void ForgotPasswordEmail(string email, int businessId)
        {
            var completeRegister = "/business/ResetPassword/" + businessId.ToString();
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, completeRegister);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com", "GrizzTime Do Not Reply");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Reset Your Password";

            string body = PopulateBody(link);

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

        private string PopulateBody(string link)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/emailTemplates/forgotpassword.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Link}", link);
            return body;
        }

        public static string Hash(string value)
        {
            return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)) 
            );
        }
    }

}
