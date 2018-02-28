using System.ComponentModel.DataAnnotations;

namespace SalaryUpdate.Models
{
    public class Employee
    {
        [Required]
        public int EmployeeId { get; set; }
        public string Status { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}";

        public int EmployerId { get; set; }
        public string EmployerName { get; set; } // never used

        public decimal Rate { get; set; } // never used

        public decimal Hours { get; set; }
        public decimal CashSalary { get; set; }
        public decimal MealAllowanceProvided { get; set; }
        public decimal CashMealAllowance { get; set; }
        public decimal CashCellphoneAllowance { get; set; }

        public decimal TotalSalary => CashSalary + MealAllowanceProvided + CashMealAllowance + CashCellphoneAllowance;
    }
}