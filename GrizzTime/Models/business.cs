
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
    
public partial class business
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public business()
    {

        this.employees = new HashSet<employee>();

        this.contracts = new HashSet<contract>();

    }


    public int UserID { get; set; }

    public string UserEmail { get; set; }

    public string UserPW { get; set; }

    public string UserStatus { get; set; }

    public string BusName { get; set; }

    public string BusDesc { get; set; }

    public string BusAddress { get; set; }

    public bool RememberMe { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<employee> employees { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<contract> contracts { get; set; }

}

}
