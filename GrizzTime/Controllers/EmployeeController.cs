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
using GrizzTime.ViewModels;
using GrizzTime.Models;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System.IO;

namespace GrizzTime.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(employee employee, String ReturnUrl)
        {
            using (Entities dc = new Entities())
            {
                var v = dc.employees.Where(a => a.UserEmail == employee.UserEmail).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Hash(employee.UserPW), v.UserPW) == 0)
                    {
                        var claims = new List<Claim>();

                        try
                        {
                            claims.Add(new Claim(ClaimTypes.Name, v.UserEmail));
                            var claimIdentities = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                            var ctx = Request.GetOwinContext();
                            var authenticationManager = ctx.Authentication;

                            authenticationManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties() { IsPersistent = false }, claimIdentities);

                            bool LRememberMe = employee.RememberMe;
                            int timeout = LRememberMe ? 52600 : 20; // Remembers for one year
                            var ticket = new FormsAuthenticationTicket(employee.UserEmail, LRememberMe, timeout);
                            string encrypted = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                            cookie.Expires = DateTime.Now.AddMinutes(timeout);
                            cookie.HttpOnly = true;
                            Response.Cookies.Add(cookie);

                            Response.Cookies.Add(new HttpCookie("UserID", v.UserID.ToString()));
                            Response.Cookies.Add(new HttpCookie("Role", v.EmpType));

                        }
                        catch (Exception ex)
                        {
                            TempData["message"] = "Something went wrong.";
                            throw ex;
                        }

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
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
                    ModelState.AddModelError("NotExist", "There is no employee account associated with this email address.");                   
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
                    var emp = dc.employees.Where(a => a.UserEmail == UserEmail).FirstOrDefault();

                    if (emp == null)
                    {
                        TempData["message"] = "Something went wrong with SQL query.";
                        return View();
                    }

                    ForgotPasswordEmail(UserEmail, emp.UserID);
                    TempData["message"] = "An email was sent to " + UserEmail + ". Please check your email.";
                }
            }
            else
            {
                TempData["message"] = "There is no account registered with that email address!";
            }
            
            return View();
        }

        public ActionResult Dashboard()
        {
            string message;
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

                int id = Int32.Parse(Request.Cookies["UserID"].Value);
            
                try
                {
                    Entities dc = new Entities();
                    //int id = Int32.Parse(Request.Cookies["UserID"].Value);
                    var v = dc.employees.Where(a => a.UserID == id).FirstOrDefault();

                    ViewBag.EmployeeFName = v.EmpFName;
                    ViewBag.EmployeeName = v.EmpFName + " " + v.EmpLName;
                    ViewBag.BusID = v.BusCode;
                    ViewBag.UserID = v.UserID;

                }
                catch (NullReferenceException e)
                { //Redirect to login if it can't find business name
                    message = "Employee object not found.";
                    System.Diagnostics.Debug.WriteLine("User is logged in, but employee is not found." + e.Message);
                    return HttpNotFound();
                }           

            return View();

        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult Create()
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find business id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }
            ViewBag.UserID = Request.Cookies["UserID"].Value;

            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Exclude = "IsEmailVerified,ActivationCode")] Employee thisEmp)
        {
            bool Status = false;
            string message;

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find business id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            int id = Int32.Parse(Request.Cookies["UserID"].Value);
            

            ModelState.Remove("UserPW");
            ModelState.Remove("ConfirmPassword");

            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists
                var isExist = IsEmailExist(thisEmp.UserEmail);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "An employee with this email address already exists.");
                    return View(thisEmp);
                }

                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.employee emp = new GrizzTime.Models.employee();                   
                    emp.UserEmail = thisEmp.UserEmail;
                    emp.EmpFName = thisEmp.EmpFName;
                    emp.EmpLName = thisEmp.EmpLName;
                    emp.EmpPhone = thisEmp.EmpPhone;
                    emp.EmpType = thisEmp.EmpType;
                    emp.SupervisorID = thisEmp.SupervisorID;
                    emp.BusCode = id;
                    emp.UserStatus = "Registered";

                    dc.employees.Add(emp);
                    dc.SaveChanges();

                    //Get id of employee just created.
                    var justCreated = dc.employees.Where(a => a.UserEmail == thisEmp.UserEmail).FirstOrDefault();

                    if (justCreated == null)
                    {
                        TempData["message"] = "Something went wrong with SQL query.";
                        return View();
                    }

                    SendRegistrationEMail(thisEmp.UserEmail, justCreated.UserID);
                }

                message = "A link to finish registration was sent to the employee.";
                Status = true;
            }
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            ViewBag.UserID = Request.Cookies["UserID"].Value;
            ViewBag.Status = Status;

            return RedirectToAction("Create", "Employee");
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

                employee employee = dc.employees.Find(id);

                if (employee == null)
                {
                    return HttpNotFound();
                }

                Employee viewEmp = new Employee()
                {
                    UserEmail = employee.UserEmail,
                    EmpFName = employee.EmpFName,
                    EmpLName = employee.EmpLName,
                    EmpPhone = employee.EmpPhone,
                    EmpType = employee.EmpType
                };


                return View(viewEmp);
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(Employee thisEmp, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string message;

            //Don't check include in validation check
            ModelState.Remove("UserEmail");
            ModelState.Remove("EmpFName");
            ModelState.Remove("EmpLName");
            ModelState.Remove("EmpPhone");
            ModelState.Remove("EmpType");

            if (ModelState.IsValid)
            {
                
                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.employee emp = dc.employees.FirstOrDefault(p => p.UserID == id);
                    if (thisEmp == null)
                        return HttpNotFound();

                    emp.UserPW = Hash(thisEmp.UserPW); 
                    emp.UserStatus = "Activated";

                    dc.Entry(emp).State = System.Data.Entity.EntityState.Modified;
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
                return RedirectToAction("Login", "Employee");
            }
            else
            {
                message = "Couldn't complete request.";
            }

            //SendVerificationEMail(thisEmp.UserEmail);
            TempData["message"] = message;
            return View(thisEmp);
        }

        [AllowAnonymous]
        public ActionResult AllEmployees()
        {
            Entities db = new Entities();
            return View(from employee in db.employees select employee);
        }

        public ActionResult Details(int? id)
        {
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                employee emp = dc.employees.Find(id);

                //-------Determine who's accessing employee details-----------
                string currentUserID = Request.Cookies["UserID"].Value;
                string type = "otherEmployee";
                if (currentUserID == emp.BusCode.ToString())
                {
                    type = "thisBusiness";
                }
                else
                {
                    if (currentUserID == emp.UserID.ToString())
                    {
                        type = "thisEmployee";
                    }
                }
                ViewBag.type = type;

                //------------------------------------------------------------
                List<Timesheet> theseTimesheets = new List<Timesheet>();
                foreach (var ts in emp.timesheets)
                {
                    theseTimesheets.Add(
                        new Timesheet()
                        {
                            //I think this is all i need for now.
                            PayrollCycleYear = ts.payrollcycle.PayrollCycleYear.ToString(),
                        }
                   ) ;
                }
                //--------------------------------------------------------------------------


                Employee viewEmp = new Employee()
                {
                    EmpFName = emp.EmpFName,
                    EmpLName = emp.EmpLName,
                    EmpType = emp.EmpType,
                    UserEmail = emp.UserEmail,
                    EmpPayRate = emp.EmpPayRate,
                    EmpPhone = emp.EmpPhone,
                    UserID = emp.UserID,
                    SupervisorID = emp.SupervisorID, 
                    SupervisorName = emp.employee2.EmpFName + " " + emp.employee2.EmpLName,
                    BusinessName = emp.business.BusName,
                    EmployeeTimesheets = theseTimesheets,
                };

                if (emp == null)
                {
                    return HttpNotFound();
                }

                return View(viewEmp);
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find employee
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

                employee emp = dc.employees.Find(id);
                Employee viewEmp = new Employee()
                {
                    UserEmail = emp.UserEmail,
                    UserID = emp.UserID,                 
                    EmpFName = emp.EmpFName,
                    EmpLName = emp.EmpLName,
                    EmpPhone = emp.EmpPhone,
                    EmpType = emp.EmpType,
                    BusCode = emp.BusCode,
                    //UserPW = emp.UserPW, //Move to its own form
                    SupervisorID = emp.SupervisorID,
                };

                if (emp == null)
                {
                    return HttpNotFound();
                }

                return View(viewEmp);
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit(int? id, Employee thisEmp)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find employee id
                TempData["message"] = "Please log in";
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
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("UserPW");

            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.employee emp = dc.employees.FirstOrDefault(p => p.UserID == id);
                    if (thisEmp == null)
                        return HttpNotFound();

                    emp.UserEmail = thisEmp.UserEmail;
                    //emp.UserPW = Hash(thisEmp.UserPW); //move to its own form
                    emp.EmpFName = thisEmp.EmpFName;
                    emp.EmpLName = thisEmp.EmpLName;
                    emp.EmpPhone = thisEmp.EmpPhone;
                    emp.EmpType = thisEmp.EmpType;
                    emp.SupervisorID = thisEmp.SupervisorID;

                    dc.Entry(emp).State = System.Data.Entity.EntityState.Modified;
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
                message = "Employee updated successfully.";
                Status = true;
                TempData["message"] = message;
                ViewBag.Status = Status;
                if (Request.Cookies["Role"].Value.Equals("Business"))
                    return RedirectToAction("MyEmployees", "Business");
                else
                    return RedirectToAction("Dashboard", "Employee");
            }
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            ViewBag.Status = Status;
            return View(thisEmp);
        }

        [HttpGet]
        public ActionResult EditPayRate(int? id)
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

                employee emp = dc.employees.Find(id);
                Employee viewEmp = new Employee()
                {
                    EmpPayRate = emp.EmpPayRate,
                };

                if (emp == null)
                {
                    return HttpNotFound();
                }

                return View(viewEmp);
            }
        }

        [HttpPost]
        public ActionResult EditPayRate(int? id, Employee thisEmp)
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
            ModelState.Remove("UserEmail");
            ModelState.Remove("EmpFName");
            ModelState.Remove("EmpLName");
            ModelState.Remove("EmpPhone");
            ModelState.Remove("UserPW");
            ModelState.Remove("EmpType");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.employee emp = dc.employees.FirstOrDefault(p => p.UserID == id);
                    if (thisEmp == null)
                        return HttpNotFound();

                    emp.EmpPayRate = thisEmp.EmpPayRate;

                    dc.Entry(emp).State = System.Data.Entity.EntityState.Modified;
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
                message = "Pay rate updated successfully.";
                Status = true;
                TempData["message"] = message;
                ViewBag.Status = Status;
                return RedirectToAction("MyEmployees", "Business");
            }
            else
            {
                message = "Invalid Request";
            }

            TempData["message"] = message;
            ViewBag.Status = Status;
            return View(thisEmp);
        }

        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                employee emp = dc.employees.Find(id);

                if (emp == null)
                {
                    message = "Employee not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyEmployees", "Business");
                }

                TempData["message"] = message;
                return View(emp);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string confirmButton)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                employee employee = dc.employees.Find(id);

                if (employee == null)
                {
                    message = "Employee not found.";
                    TempData["message"] = message;
                    return RedirectToAction("MyEmployees", "Business");
                }

                dc.employees.Remove(employee);
                dc.SaveChanges();
                message = "Employee successfully deleted.";
            }
            TempData["message"] = message;
            return RedirectToAction("MyEmployees", "Business");
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
            string subject = "Your account has been succesfully created!";

            string body = "<br/><br/> We are excited to tell you that you're GrizzTime account has been created!";

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
            var completeRegister = Url.Action("ResetPassword", "Employee", new { id = employeeId }); /*"/employee/registration/" + employeeId.ToString(); */
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, completeRegister);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com", "GrizzTime Do Not Reply");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "WinterSemester";
            string subject = "Your GrizzTime account has been succesfully created!";

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
        public void ForgotPasswordEmail(string email, int employeeId)
        {
            var forgotPassword = Url.Action("ResetPassword", "Employee", new { id = employeeId });/*"/employee/registration/" + employeeId.ToString();*/
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, forgotPassword);

            //Try to make secure later.
            //var forgotPassword = Url.Action("Registration", "Employee", new { id = employeeId.ToString() }, protocol: Request.Url.Scheme);
            //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, forgotPassword);

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

        public static string Hash(string value)
        {
            return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value))
                );
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

        private string PopulateBody2(string link, string message)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/emailTemplates/forgotpassword.html")))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{Link}", link);
            return body;
        }

    }

}