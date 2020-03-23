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
            new SelectListItem() {Text="Transportation", Value = "Transportation" },
            new SelectListItem() {Text="Food", Value="Food" },
            new SelectListItem() {Text="Housing", Value="Housing" },
            new SelectListItem() {Text="Travel Supplies", Value="Travel Supplies" },
            new SelectListItem() {Text="Airfare", Value="Airfare" },
            new SelectListItem() {Text="Tech Support", Value="Tech Support" },
            new SelectListItem() {Text="Other", Value="Other" },

        };
     


    }

}