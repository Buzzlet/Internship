using SalaryUpdate.Persistance.SourceModels;
using System;
using System.Data.Entity;

namespace SalaryUpdate.Persistance
{
    /// <summary>
    /// This was a Database First context and has been removed.
    /// You will need to rebuild this context.
    /// </summary>
    public class SourceContext // : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<CustomField> CustomFields { get; set; }
        public DbSet<EmployeeExtended> EmployeesExtended { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<EmployeeActions> EmployeeActions { get; set; }
        public DbSet<SourceModels.System> Systems { get; set; }
    }
}

namespace SalaryUpdate.Persistance.SourceModels
{
    public class Employee
    {
        public short LocationId { get; set; }
        public int EmployeeId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Status { get; set; }
        public int EmployerId { get; set; }
        public decimal Rate { get; set; }
        public decimal Hours { get; set; }
    }

    public class CustomField
    {
        public short LocationId { get; set; }
        public string Type { get; set; }
        public decimal ObjectId { get; set; }
        public string Key { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AltValue { get; set; }
    }

    public class EmployeeExtended
    {
        public short LocationId { get; set; }
        public int EmployeeId { get; set; }
        public decimal Salary1 { get; set; }
        public decimal Salary2 { get; set; }
        public decimal Salary3 { get; set; }
        public decimal Salary4 { get; set; }
    }

    public class Position
    {
        public short LocationId { get; set; }
        public string Value { get; set; }
        public decimal ObjectId { get; set; }
    }

    public class EmployeeActions
    {
        public short LocationId { get; set; }
        public int EmployeeId { get; set; }
        public string Action { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class System
    {
        public short LocationId { get; set; }
        public int EmployerId { get; set; }
        public string Name { get; set; }
    }
}