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
            string message = "";
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
                            message = "Failed try.";
                            throw ex;
                        }

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Dashboard");
                        }
                    }
                }
                else
                {
                    message = "Invalid credentials";
                    return View();
                }
            }

            ViewBag.Message = message;
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            string message = "";
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                ViewBag.Message = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

                int id = Int32.Parse(Request.Cookies["UserID"].Value);
            
                try
                {
                    Entities dc = new Entities();
                    //int id = Int32.Parse(Request.Cookies["UserID"].Value);
                    var v = dc.employees.Where(a => a.UserID == id).FirstOrDefault();

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

        public ActionResult AddEmployeePopUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddEmployeePopUp([Bind(Exclude = "IsEmailVerified,ActivationCode")] Employee thisEmp)
        {
            bool Status = false;
            string message;

            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find business id
                message = "Please log in.";
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

                    emp.BusCode = id;
                    emp.UserStatus = "Registered";

                    dc.employees.Add(emp);
                    dc.SaveChanges();

                    SendRegistrationEMail(thisEmp.UserEmail, emp.UserID);
                }

                message = "A link to finish registration was sent to the employee.";
                Status = true;
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;

            return View(thisEmp);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Registration(int? id)
        {
            using (Entities dc = new Entities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                employee employee = dc.employees.Find(id);
             Employee viewEmp = new Employee()
                {
                    UserEmail = employee.UserEmail,
                    EmpFName = employee.EmpFName,
                    EmpLName = employee.EmpLName,
                    EmpPhone = employee.EmpPhone,
                    EmpType = employee.EmpType
                };

                if (employee == null)
                {
                    return HttpNotFound();
                }

                return View(viewEmp);
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration(Employee thisEmp, int? id)
        {
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
                message = "Registration complete! An email has been sent to you to confirm your registration!";
                Status = true;
            }
            else
            {
                message = "Invalid Request";
            }

            SendVerificationEMail(thisEmp.UserEmail);
            ViewBag.Message = message;
            ViewBag.Status = Status;
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
                Employee viewEmp = new Employee()
                {
                    EmpFName = emp.EmpFName,
                    EmpLName = emp.EmpLName,
                    EmpType = emp.EmpType,
                    UserEmail = emp.UserEmail,
                    EmpPayRate = emp.EmpPayRate,
                    EmpPhone = emp.EmpPhone,
                    UserID = emp.UserID,
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
                ViewBag.Message = "Please log in.";
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
                    UserPW = emp.UserPW,
                    EmpFName = emp.EmpFName,
                    EmpLName = emp.EmpLName,
                    EmpPhone = emp.EmpPhone,
                    EmpType = emp.EmpType,
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
                ViewBag.Message = "Please log in";
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

            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    GrizzTime.Models.employee emp = dc.employees.FirstOrDefault(p => p.UserID == id);
                    if (thisEmp == null)
                        return HttpNotFound();

                    emp.UserEmail = thisEmp.UserEmail;
                    emp.UserPW = Hash(thisEmp.UserPW);
                    emp.EmpFName = thisEmp.EmpFName;
                    emp.EmpLName = thisEmp.EmpLName;
                    emp.EmpPhone = thisEmp.EmpPhone;
                    emp.EmpType = thisEmp.EmpType;                  

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
                ViewBag.Message = message;
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

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(thisEmp);
        }

        [HttpGet]
        public ActionResult EditPayRate(int? id)
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                ViewBag.Message = "Please log in.";
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
                ViewBag.Message = "Please log in.";
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
                ViewBag.Message = message;
                ViewBag.Status = Status;
                return RedirectToAction("MyEmployees", "Business");
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
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
                    ViewBag.Message = message;
                    return RedirectToAction("MyEmployees", "Business");
                }

                ViewBag.Message = message;
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
                    ViewBag.Message = message;
                    return RedirectToAction("MyEmployees", "Business");
                }

                dc.employees.Remove(employee);
                dc.SaveChanges();
                message = "Employee successfully deleted.";
            }
            ViewBag.Message = message;
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