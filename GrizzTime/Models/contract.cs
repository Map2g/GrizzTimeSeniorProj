
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
    
public partial class contract
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public contract()
    {

        this.projects = new HashSet<project>();

    }


    public int ConID { get; set; }

    public string ConName { get; set; }

    public Nullable<decimal> ConAllottedHours { get; set; }

    public int BusID { get; set; }

    public decimal ConHoursRemaining { get; set; }



    public virtual business business { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<project> projects { get; set; }

}

}
