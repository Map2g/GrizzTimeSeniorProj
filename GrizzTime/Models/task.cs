
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
    
public partial class task
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public task()
    {

        this.workentries = new HashSet<workentry>();

    }


    public int TaskID { get; set; }

    public string TaskName { get; set; }

    public bool IsBillable { get; set; }

    public Nullable<decimal> BillableRate { get; set; }

    public int ProjID { get; set; }



    public virtual project project { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<workentry> workentries { get; set; }

}

}
