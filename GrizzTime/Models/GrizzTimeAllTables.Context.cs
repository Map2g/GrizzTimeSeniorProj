﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<business> businesses { get; set; }
        public virtual DbSet<employee> employees { get; set; }
        public virtual DbSet<employee_project> employee_project { get; set; }
        public virtual DbSet<emppayrate_history> emppayrate_history { get; set; }
        public virtual DbSet<emptype_history> emptype_history { get; set; }
        public virtual DbSet<expenseentry> expenseentries { get; set; }
        public virtual DbSet<expensesheet> expensesheets { get; set; }
        public virtual DbSet<payrollcycle> payrollcycles { get; set; }
        public virtual DbSet<project> projects { get; set; }
        public virtual DbSet<ptorequest> ptorequests { get; set; }
        public virtual DbSet<ptorequestday> ptorequestdays { get; set; }
        public virtual DbSet<timesheet> timesheets { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<workentry> workentries { get; set; }
    }
}
