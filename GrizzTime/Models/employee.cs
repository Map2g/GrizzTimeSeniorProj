
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
    
public partial class employee
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public employee()
    {

        this.employee_project = new HashSet<employee_project>();

        this.emppayrate_history = new HashSet<emppayrate_history>();

        this.emptype_history = new HashSet<emptype_history>();

        this.expensesheets = new HashSet<expensesheet>();

        this.ptorequests = new HashSet<ptorequest>();

        this.timesheets = new HashSet<timesheet>();

        this.projects = new HashSet<project>();

    }


    public int UserID { get; set; }

    public string UserEmail { get; set; }

    public string UserPW { get; set; }

    public string UserStatus { get; set; }

    public string EmpFName { get; set; }

    public string EmpLName { get; set; }

    public string EmpType { get; set; }

    public int BusCode { get; set; }

    public decimal EmpPayRate { get; set; }

    public string EmpPhone { get; set; }

    public bool RememberMe { get; set; }



    public virtual business business { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<employee_project> employee_project { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<emppayrate_history> emppayrate_history { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<emptype_history> emptype_history { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<expensesheet> expensesheets { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<ptorequest> ptorequests { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<timesheet> timesheets { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<project> projects { get; set; }

}

}
