using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class ExpenseEntry
    {


        public int ExpenseEntryID { get; set; }

        public int EmpID { get; set; }

        public string EmpName { get; set; }

        public int ProjID { get; set; }

        public int ProjName { get; set; }

        //Day of the week
        public String ExpDate { get; set; }

        public decimal ExpDollarAmt { get; set; }

        public int ExpSheetID { get; set; }

        public string ExpCategory { get; set; }

        public string SelectedCategoryText { get; set; }

        public static List<SelectListItem> ExpenseTypes = new List<SelectListItem>()
        {
            new SelectListItem() {Text="Transportation", Value = "1" },
            new SelectListItem() {Text="Food", Value= "2" },
            new SelectListItem() {Text="Housing", Value= "3" },
            new SelectListItem() {Text="Travel Supplies", Value="4" },
            new SelectListItem() {Text="Airfare", Value = "5" },
            new SelectListItem() {Text="Tech Support", Value = "6" },
            new SelectListItem() {Text="Other", Value= "7" },

        };
     


    }

}