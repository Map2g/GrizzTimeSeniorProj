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

namespace GrizzTime.Controllers
{
    public class BusinessController : Controller
    {
        //GrizzTimeEntities db = new GrizzTimeEntities();
        // GET: Business
        public ActionResult Index()
        {
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
                var v = dc.Users.Where(a => a.email == business.UserEmail).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(business.UserPW, v.password) == 0)
                    {
                        bool LRememberMe = business.RememberMe;
                        int timeout = LRememberMe ? 52600 : 20; // Remembers for one year
                        var ticket = new FormsAuthenticationTicket(business.UserEmail, LRememberMe, timeout);
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
        public ActionResult Edit(int? UserID)
        {
            Entities dc = new Entities();

            if (UserID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            business business1 = dc.businesses.Find(UserID);
            if (business1 == null)
            {
                return HttpNotFound();
            }

            return View(business1);

        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind(Include = "UserID, BusName,BusAddress,BusDesc,UserPW, UserEmail")]business business1)
        {
            if (ModelState.IsValid)
            {
                using (Entities dc = new Entities())
                {
                    dc.Entry(business1).State = System.Data.Entity.EntityState.Modified;
                    dc.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(business1);

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
                var v = dc.businesses.Where(a => a.UserEmail == emailID).FirstOrDefault();
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

            string body = "<br/><br/> We are excited to tell you that you're GrizzTime business account has been created!...";

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
    }

}
