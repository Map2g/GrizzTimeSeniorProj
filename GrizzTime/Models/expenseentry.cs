
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace GrizzTime.Models
{

using System;
    using System.Collections.Generic;
    
public partial class expenseentry
{

    public int ExpEntryID { get; set; }

    public int EmpID { get; set; }

    public int ProjID { get; set; }

    public System.DateTime ExpDate { get; set; }

    public decimal ExpDollarAmt { get; set; }

    public string ExpType { get; set; }

    public int ExpSheetID { get; set; }



    public virtual expensesheet expensesheet { get; set; }

}

}
