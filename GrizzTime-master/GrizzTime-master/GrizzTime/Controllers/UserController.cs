using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using GrizzTime.Models;

namespace GrizzTime.Controllers
{
    public class UserController : Controller
    {
        //GrizzTimeEntities db = new GrizzTimeEntities();
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Registration([Bind(Exclude ="IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
            //ensure that the model exists
            if (ModelState.IsValid)
            {
                //Email already exists
                /*var isExist = IsEmailExist(user.email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exists");
                    return View(user);
                }*/
                //Save to Database
                using (GrizzTimeEntities5 dc = new GrizzTimeEntities5())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //send email to User
                    SendVerificationEMail(user.email);
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
            //if (ModelState.IsValid){
            //    using (GrizzTimeEntities3 dc = new GrizzTimeEntities3())
            //    {
            //        dc.Users.
            //    }
            //}
            
                return View();
        }

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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
        public void SendVerificationEMail(string email)
        {
            var verifyUrl = "/User/VerifyAccount";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("grizztimenotification@gmail.com", "Grizz Time Team");
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
    }
    
}
