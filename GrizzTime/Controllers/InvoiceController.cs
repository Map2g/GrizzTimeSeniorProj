using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.Controllers
{
    public class InvoiceController : Controller
    {
        public ActionResult EmployeeInvoice(int eid)
        {
            return View();
        }

        public ActionResult ProjectInvoice(int pid)
        {
            return View();
        }
    }
}