
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
    
public partial class ptorequestday
{

    public int PTORequestDaysID { get; set; }

    public int PTORequestID { get; set; }

    public System.DateTime PTORequestDay1 { get; set; }



    public virtual ptorequest ptorequest { get; set; }

}

}
