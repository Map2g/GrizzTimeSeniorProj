using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace GrizzTime.Models
{
    public class UserDB
    {[Key]
        public int UserID { get; set; }
        public string firstname { get; set; }
        public string lastName { get; set; }
        public string email{ get; set; }
        public string password { get; set; }
        public string phone { get; set; }
    }
}