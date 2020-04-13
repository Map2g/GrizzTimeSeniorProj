using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class Expensesheet
    {
        public int ExpSheetID { get; set; }

        public int PayrollCycleID { get; set; }

        public int EmpID { get; set; }

        [Display(Name = "Week")]
        public System.DateTime Week { get; set; }

        public string ExpSheetStatus { get; set; }

        public decimal ExpSheetTotalAmt { get; set; }

        public Nullable<System.DateTime> ExpSheetSubmitTime { get; set; }

        public Nullable<System.DateTime> ExpSheetApproveTime { get; set; }

        public List<ExpenseEntry> thisExpenseEntries { get; set; }

    
    }

}