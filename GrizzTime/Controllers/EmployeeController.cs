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
        //GrizzTimeEntities db = new GrizzTimeEntities();
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Registration()
        {
            //Dictionary<int, string> myJobList = new Dictionary<int, string>();
            //myJobList.Add(0, "President"); myJobList.Add(1, "CEO"); myJobList.Add(2, "CTO"); myJobList.Add(3, "CIO"); myJobList.Add(4, "Director"); myJobList.Add(5, "Projecy Manager"); myJobList.Add(6, "Technology Lead"); myJobList.Add(7, "Software Engineer"); myJobList.Add(8, "Intern");
            //ViewBag.JobList = myJobList.GetEnumerator();

            //model.CategoryList = new SelectList(db.Categories, "ID", "Name");
            //List<string> JobNames = new List<string> { "President", "CEO", "CFO", "CTO", "CIO", "Director", "Project Manager", "Technology Lead", "Software Engineer", "Intern" };
            //ViewBag.JobList = new SelectList(JobNames);
            return View();
        }


        [HttpPost]
        public ActionResult Registration([Bind(Exclude ="IsEmailVerified,ActivationCode")] employee employee)
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
                    ModelState.AddModelError("EmailExist", "Email already exists");
                    return View(employee);
                }

                //Check validity of business code
                Entities dc = new Entities();
                var validBusiness = from business in dc.businesses
                                      where business.UserID == employee.BusCode
                                      select business;
                if (validBusiness.Count() != 1 )
                {
                    ModelState.AddModelError("InvalidBusinesscode", "That business code does not exist");
                    return View(employee);
                }

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
                SendVerificationEMail(employee.UserEmail);
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
        public ActionResult Login(employee employee, String ReturnUrl)
        {
            string message = "";
            using (Entities dc = new Entities())
            {
                var v = dc.Users.Where(a => a.email == employee.UserEmail).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(employee.UserPW,v.password) == 0){
                        bool LRememberMe = employee.RememberMe;
                        int timeout = LRememberMe ? 52600 : 20; // Remembers for one year
                        var ticket = new FormsAuthenticationTicket(employee.UserEmail, LRememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if(Url.IsLocalUrl(ReturnUrl))
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
            return RedirectToAction("Login", "User");
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
            Entities dc = new Entities();
            
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                employee _employee = dc.employees.Find(id);
                if (_employee == null)
                {
                    return HttpNotFound();
                }

            return View(_employee);
            
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind(Include ="UserID,EmpFName,EmpLName,UserEmail,UserPW, EmpType, EmpPhone")]employee employee)
        {
            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    dc.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    dc.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
            
        }

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: User/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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
        public static  string Hash(string value)
        {
            return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value))
                );
        }
    }
    
}
