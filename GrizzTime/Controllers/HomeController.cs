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
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ChooseAccountType()
        {
            ViewBag.Message = "Your pre-registration page.";

            return View();
        }

        [HttpPost]
        public ActionResult ChooseAccountType(String ReturnUrl)
        {
            string accountType = Request.Form["accountType"];
            switch (accountType) 
            {
                case "business":
                    RedirectToAction("Registration", "Business");
                    break;
                case "employee":
                    RedirectToAction("Registration", "Employee");
                    break;
                default:
                    Console.WriteLine("Default case");
                    Redirect(ReturnUrl);
                    break;
            }
            return View();

            //if (Url.IsLocalUrl(ReturnUrl))
            //{
            //    return Redirect(ReturnUrl);
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Home");
            //}
        }
    }
}