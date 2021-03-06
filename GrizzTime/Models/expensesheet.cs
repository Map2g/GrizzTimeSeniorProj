
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
    
public partial class expensesheet
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public expensesheet()
    {

        this.expenseentries = new HashSet<expenseentry>();

    }


    public int ExpSheetID { get; set; }

    public int PayrollCycleID { get; set; }

    public int EmpID { get; set; }

    public string ExpSheetStatus { get; set; }

    public Nullable<decimal> ExpSheetTotalAmt { get; set; }

    public Nullable<System.DateTime> ExpSheetSubmitTime { get; set; }

    public Nullable<System.DateTime> ExpSheetApproveTime { get; set; }



    public virtual employee employee { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<expenseentry> expenseentries { get; set; }

    public virtual payrollcycle payrollcycle { get; set; }

}

}
