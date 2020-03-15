﻿using GrizzTime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GrizzTime.ViewModels;
using System.Net;
using System.Data.SqlClient;

namespace GrizzTime.Controllers
{
    public class ExpensesheetController : Controller
    {
        public ActionResult List(string m)
        {
            string message = m;

            if (!Request.Cookies.AllKeys.Contains("UserID"))
            {
               /// Redirect to login if it can't find user id
                message = "Please log in.";
               ViewBag.Message = message;
                System.Diagnostics.Debug.WriteLine("User not logged in. Redirecting to login page.\n");
                return RedirectToAction("LandingPage", "Home");
            }

            try
            {
                IEnumerable<expensesheet> thisEmployeeExpenseSheet;
                Entities dc = new Entities();

                int id = Int32.Parse(Request.Cookies["UserID"].Value);
                thisEmployeeExpenseSheet = dc.expensesheets.Where(x => x.EmpID == id).OrderByDescending(p => p.payrollcycle.PayrollCycleStart).ToList();

                if (thisEmployeeExpenseSheet.Any() == false)
                {
                    message = "You don't have any timesheets yet!";
                    ViewBag.Message = message;
                    return View();
                }
                else
                {
                    ViewBag.Message = message;
                    return View(thisEmployeeExpenseSheet);
                }

            }
            catch (Exception ex)
            {
                message = "something happened";
                ViewBag.Message = message;
                return View();
                throw ex;
            }

        }





        [HttpGet]
        public ActionResult ExpenseEntry()
        {

            using (Entities dc = new Entities())
            {
                ViewBag.UserID = Int32.Parse(Request.Cookies["UserID"].Value);
                var d = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday).Date;
                var v = dc.payrollcycles.Where(a => a.PayrollCycleStart == d ).FirstOrDefault();
                if (v == null)
                {
                    GrizzTime.Models.payrollcycle pc = new GrizzTime.Models.payrollcycle();
                    pc.PayrollCycleStart = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday).Date;
                    pc.PayrollCycleEnd = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(7).Date;
                    pc.PayrollCycleYear = (short)System.DateTime.Now.Year;
                    dc.payrollcycles.Add(pc);
                    dc.SaveChanges();
                }
                ViewBag.IsChangeable = true;
                return View();

            }
        }




        [HttpPost]
        public ActionResult ExpenseEntry(ExpenseEntry thisExpense)
        {
            using (Entities dc = new Entities())
            {
                DateTime d = System.DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                IEnumerable<payrollcycle> thispayroll;
                ///payrollcycle pc = dc.payrollcycles.Where(a => a.PayrollCycleStart = d);
                var v = dc.payrollcycles.Where(a => a.PayrollCycleStart == d).FirstOrDefault();

                expensesheet es = new expensesheet();
                es.EmpID = Int32.Parse(Request.Cookies["UserID"].Value);
                es.PayrollCycleID = v.PayrollCycleID;
                es.ExpSheetStatus = "";
                es.ExpSheetTotalAmt = (int)0;

                dc.expensesheets.Add(es);
                dc.SaveChanges();


                expenseentry ee = new expenseentry()
                {
                    ExpSheetID = (int)es.ExpSheetID,
                    EmpID = es.EmpID,
                    ExpDate = thisExpense.ExpDate,
                    ExpDollarAmt = thisExpense.ExpDollarAmt,
                    TaskID = thisExpense.TaskID,
                    ProjID = thisExpense.ProjID
                };

                dc.expenseentries.Add(ee);
                dc.SaveChanges();
                return View("List");


            }
        }
    }
}





