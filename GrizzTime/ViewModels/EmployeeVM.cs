using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace GrizzTime.ViewModels
{
    public class EmployeeVM
    {
        public enum JobType
        {
            [Description("President")]
            President,
            [Description("CEO")]
            CEO,
            [Description("CTO")]
            CTO,
            [Description("CIO")]
            CIO,
            [Description("CFO")]
            CFO,
            [Description("Director")]
            Director,
            [Description("Project Manager")]
            ProjectManager,
            [Description("Technology Lead")]
            TechnologyLead,
            [Description("Software Engineer")]
            SoftwareEngineer,
            [Description("Intern")]
            Intern
        }

        public int UserID { get; set; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "The email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string UserEmail { get; set; }

        [Display(Name = "Password")]
        [MembershipPassword(
                MinRequiredNonAlphanumericCharacters = 1,
                MinNonAlphanumericCharactersError = "Your password needs to contain at least one symbol (!, @, #, etc).",
                MinRequiredPasswordLength = 8,
                MinPasswordLengthError = "Your password must be at least 8 characters long."
        )]
        [DataType(DataType.Password)]
        public string UserPW { get; set; }

        public string UserStatus { get; set; }

        [Required]
        public string EmpFName { get; set; }

        [Required]
        public string EmpLName { get; set; }

        [Required]
        public string EmpType { get; set; }

        public int BusCode { get; set; }

        public decimal EmpPayRate { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number is invalid.")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Phone number is invalid. (e.g. 2485550712)")]
        public string EmpPhone { get; set; }

        public bool RememberMe { get; set; }

        [NotMapped] // Does not effect with the database
        [Compare("UserPW", ErrorMessage = "The passwords do not match.")]
        [DataType("Password")]
        public string ConfirmPassword { get; set; }
    }
}