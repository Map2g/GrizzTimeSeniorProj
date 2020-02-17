using GrizzTime.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GrizzTime.BusinessLogic
{
    public class Business
    {
        public int UserID { get; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "The email address is required. ")]
        [EmailAddress(ErrorMessage = "Invalid email address. ")]
        public string UserEmail { get; set; }

        [Display(Name = "Password")]
        [MembershipPassword(
        MinRequiredNonAlphanumericCharacters = 1,
        MinRequiredPasswordLength = 8,
        ErrorMessage = "Your password must be at least 8 characters long and must contain at least one symbol (!, @, #, etc). "
        )]
        [DataType(DataType.Password)]
        public string UserPW { get; set; }

        [Display(Name = "Business Name")]
        [Required(ErrorMessage = "Business name is required. ")]
        public string BusName { get; set; }

        [Display(Name = "Description")]
        public string BusDesc { get; set; }

        [Display(Name = "Address")]
        [Required(ErrorMessage = "Address is required. ")]
        public string BusAddress { get; set; }

        public string UserStatus { get; set; }

        public void SaveNew()
        {
            //TODO : validate before save
            //TODO : automapper
            using (Entities dc = new Entities())
            {
                GrizzTime.Models.business bus = new GrizzTime.Models.business();
                bus.UserEmail = this.UserEmail;
                bus.UserPW = this.UserPW;
                bus.BusName = this.BusName;
                bus.BusDesc = this.BusDesc;
                bus.BusAddress = this.BusAddress;
                bus.UserStatus = this.UserStatus;

                dc.businesses.Add(bus);
                dc.SaveChanges();
            }
        }


        public class BusinessList : List<business>
        {
            public void Load()
            {
                using (Entities dc = new Entities())
                {
                    foreach (var bus in dc.businesses)
                    {
                        //TODO : use automapper
                        business b = new business() { UserEmail = bus.UserEmail, UserPW = bus.UserPW, BusName = bus.BusName, BusDesc = bus.BusDesc, BusAddress = bus.BusAddress, UserStatus = bus.UserStatus };
                        this.Add(b);
                    }
                }
            }

        }

    }
}