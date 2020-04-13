using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrizzTime.ViewModels
{
    public class ExpenseCategory
    {
        public string CategoryName { get; set; }

        public decimal EmpCatTotalAmt { get; set; }

        public decimal ProCatTotalAmt { get; set; }

    }
}