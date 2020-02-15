//using GrizzTime.BusinessLogic;
//using System.Collections.Generic;
//using System.Web.Mvc;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using GrizzTime.Models;

namespace GrizzTime.ViewModels
{
    public class EmployeeVM
    {
        //Gets a list of employees in a specific business. ID should be UserID of a business
        public static List<SelectListItem> EmployeeList(string id)
        {           
            string query = "SELECT * FROM grizztime.employee WHERE BusCode = @id";

            Entities dc = new Entities();
            IEnumerable<employee> thisBusinessEmployees = dc.employees.SqlQuery(query, new SqlParameter("@id", id));

            List<SelectListItem> AllEmployees = new List<SelectListItem>();

            foreach (var item in thisBusinessEmployees)
            {               
                AllEmployees.Add( new SelectListItem() { Text = item.EmpFName + " " + item.EmpLName, Value = item.UserID.ToString() } ); 
            };

            return AllEmployees;
        }
    }
}




//    public class EmployeeVM
//    {
//        public static ViewModelBusinessList ListOfBusinesses
//        {
//            get
//            {
//            TODO: put this into cache to not load every time
//           ViewModelBusinessList dl = new ViewModelBusinessList();
//                dl.Load();
//                return dl;
//            }
//        }
//    }
//    public class ViewModelEmployeeCreate : EmployeeVM
//    {
//        public Employee emp = new Employee();
//    }
//    public class ViewModelEmployeeList : EmployeeVM
//    {
//        public Employee.EmployeeList employees;
//        public void Load()
//        {
//            employees = new Employee.EmployeeList();
//            employees.Load();
//        }
//    }