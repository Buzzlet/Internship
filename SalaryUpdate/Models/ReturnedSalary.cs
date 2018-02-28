using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace SalaryUpdate.Models
{
    public class ReturnedSalary
    {
        public string EmployeeId { get; set; }
        public int EmployerId { get; set; }

        [Display(Name = "Total Salary"), Range(0.01, 1000000, ErrorMessage = "{0} must be greater than $0.00 and less than $1,000,000.00!")]
        public decimal NewTotalSalary { get; set; }
        [Min(0)] public decimal NewBasicCash { get; set; }
        [Min(0)] public decimal NewMealsProvided { get; set; }
        [Min(0)] public decimal NewCashMealAllowance { get; set; }
        [Min(0)] public decimal NewCashCellphoneAllowance { get; set; }
        [Display(Name = "Hours"), Range(20.01, 60, ErrorMessage = "{0} must be greater than 20 and less than {2}!")]
        public decimal NewHoursPerWeek { get; set; }

        public bool Exported { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime? DateExported { get; set; }
    }
}