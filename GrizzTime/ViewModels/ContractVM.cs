using GrizzTime.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrizzTime.ViewModels
{
    public class Contract
    {
        public int ConID { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "This field is required. ")]
        public string ConName { get; set; }

        [Display(Name = "Alloted Hours")]
        [Required(ErrorMessage = "This field is required. ")]
        public decimal ConAllottedHours { get; set; }

        public decimal ConHoursRemaining { get; set; }

        public decimal BusinessOwnerID { get; set; }

        public string BusinessName { get; set; }

        //Gets a list of contracts for a specific business. ID should be UserID of a business
        public static List<SelectListItem> ContractList(string id)
        {
            string query = "SELECT * FROM grizztime.contract WHERE BusID = @id";

            Entities dc = new Entities();
            IEnumerable<contract> thisBusinessContracts = dc.contracts.SqlQuery(query, new SqlParameter("@id", id));

            List<SelectListItem> AllContracts = new List<SelectListItem>();

            foreach (var item in thisBusinessContracts)
            {
                AllContracts.Add(new SelectListItem() { Text = item.ConName, Value = item.ConID.ToString() });
            };

            return AllContracts;
        }

    }
}