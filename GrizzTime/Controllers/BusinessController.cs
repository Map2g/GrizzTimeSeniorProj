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
using System.Text;
using GrizzTime.BusinessLogic;

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
                Entities dc = new Entities();
                int id = Int32.Parse(Request.Cookies["UserID"].Value);
                var v = dc.businesses.Where(a => a.UserID == id).FirstOrDefault();
                ViewBag.BusinessName = v.BusName;

                ViewBag.BusinessID = Request.Cookies["UserID"].Value;
            }
            catch (NullReferenceException e)
            { //Redirect to login if it can't find business name
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n" + e.Message);
                return RedirectToAction("Login", "Business");
            }

            return View();
        }

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] Business thisBus)
        {
            bool Status = false;
            string message = "";
            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists
                var isExist = IsEmailExist(thisBus.UserEmail);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exists.");
                    return View(thisBus);
                }
                //Save to Database
                try
                {
                    using (Entities dc = new Entities())
                    {
                        GrizzTime.Models.business bus = new GrizzTime.Models.business();
                        bus.UserEmail = thisBus.UserEmail;
                        bus.UserPW = thisBus.UserPW;
                        bus.BusName = thisBus.BusName;
                        bus.BusDesc = thisBus.BusDesc;
                        bus.BusAddress = thisBus.BusAddress;
                        
                        bus.UserStatus = "Registered";

                        dc.businesses.Add(bus);
                        dc.SaveChanges();
                    }

                    SendVerificationEMail(thisBus.UserEmail);
                    message = "Registration complete! An email has been sent to you to confirm your registration!";
                    Status = true;

                    return RedirectToAction("Details");
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
                        //Response.Cookies.Add(new HttpCookie("BusinessName", v.BusName ));



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
                    GrizzTime.Models.business bus = dc.businesses.FirstOrDefault(p => p.UserID == id);
                    if (thisBus == null)
                        return HttpNotFound();

                    bus.BusName = thisBus.BusName;
                    bus.BusAddress = thisBus.BusAddress;
                    bus.BusDesc = thisBus.BusDesc;
                    bus.UserEmail = thisBus.UserEmail;
                    bus.UserStatus = thisBus.UserStatus;
                    bus.UserPW = Hash(thisBus.UserPW);

                    dc.Entry(bus).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        dc.SaveChanges();
                        message = "Business updated successfully.";
                        Status = true;
                        ViewBag.Message = message;
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

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(thisBus);
        }

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                business business = dc.businesses.Find(id);

                if (business == null)
                {
                    message = "Business does not exist.";
                    ViewBag.message = message;
                    return View("Details");
                }
                ViewBag.message = message;
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
                    ViewBag.message = message;
                    return RedirectToAction("Details");
                }

                dc.businesses.Remove(business);
                dc.SaveChanges();
                message = "Business successfully deleted.";
            }
            ViewBag.message = message;
            return View("Details");
        }

        public ActionResult MyContracts()
        {
            var id = Request.Cookies["UserID"].Value;
            string query = "SELECT * FROM grizztime.contract WHERE BusID = @id";
            Entities dc = new Entities();

            IEnumerable<contract> thisBusinessContract = dc.contracts.SqlQuery(query, new SqlParameter("@id", id));
            return View(thisBusinessContract);
        }

        public ActionResult MyEmployees()
        {
            var id = Request.Cookies["UserID"].Value;
            string query = "SELECT * FROM grizztime.employee WHERE BusCode = @id";
            Entities dc = new Entities();

            IEnumerable<employee> thisBusinessEmployee = dc.employees.SqlQuery(query, new SqlParameter("@id", id));

            return View(thisBusinessEmployee);
        }

        public ActionResult MyProjects()
        {
            int id = Int32.Parse(Request.Cookies["UserID"].Value);
            List<Project> theseProjects = Project.BusProjList(id);

            //string query = "SELECT * FROM grizztime.project " +
            //                "INNER JOIN grizztime.employee ON grizztime.project.ProjManID = grizztime.employee.UserID " +
            //                    "WHERE ConID = (SELECT ConID FROM grizztime.contract WHERE BusID = @id)";
            //Entities dc = new Entities();

            //IEnumerable<project> thisBusinessProject = dc.Database.SqlQuery<project>.(query, new SqlParameter("@id", id));

            return View(theseProjects);

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
            var completeRegister = "/employee/registration/" + employeeId.ToString();
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, completeRegister);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com");
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

        public static string Hash(string value)
        {
            return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)) 
            );
        }
    }

}
