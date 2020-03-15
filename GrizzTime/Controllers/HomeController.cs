using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;
using GrizzTime.ViewModels;
using GrizzTime.Models;

namespace GrizzTime.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LandingPage()
        {

            return PartialView();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Logout()
        {
            //try
            //{
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;



                //older
                ExpireAllCookies();
                FormsAuthentication.SignOut();
                Session.Abandon();
                //end older
                authenticationManager.SignOut();
            //}
            //catch (Exception ex)
            //{
            //   throw ex;
            //}
            return RedirectToAction("LandingPage", "Home");
        }

        public ActionResult About()
        {         
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult MyProjects()
        {
            if (Request.Cookies["UserID"].Value == null)
            {
                //Redirect to login if it can't find user id
                TempData["message"] = "Please log in.";
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            int id = Int32.Parse(Request.Cookies["UserID"].Value);
            Entities dc = new Entities();

            List<Project> theseProjects;
            if (dc.businesses.Where(x => x.UserID == id).Any()){
                theseProjects = Project.BusProjList(id);
                ViewBag.isBusiness = true;
            }
            else
            {
                theseProjects = Project.PMProjList(id);
                theseProjects.AddRange(Employee.GetProjects(id));
                ViewBag.isBusiness = false;
            }          

            return View(theseProjects);
        }

        private void ExpireAllCookies()
        {
            if (HttpContext != null)
            {
                int cookieCount = Request.Cookies.Count;
                for (var i = 0; i < cookieCount; i++)
                {
                    var cookie = Request.Cookies[i];
                    if (cookie != null)
                    {
                        var expiredCookie = new HttpCookie(cookie.Name)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Domain = cookie.Domain
                        };
                        Response.Cookies.Add(expiredCookie); // overwrite it
                    }
                }

                // clear cookies server side
                Request.Cookies.Clear();
            }
        }

    }
}