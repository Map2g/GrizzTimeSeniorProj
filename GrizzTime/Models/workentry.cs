
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
    
public partial class workentry
{

    public int WorkEntryID { get; set; }

    public int EmpID { get; set; }

    public int ProjID { get; set; }

    public string WorkDate { get; set; }

    public decimal WorkHours { get; set; }

    public int TimeSheetID { get; set; }

    public int TaskID { get; set; }



    public virtual timesheet timesheet { get; set; }

    public virtual task task { get; set; }

    public virtual project project1 { get; set; }

}

}
