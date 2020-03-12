using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;

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