using SalaryUpdate.Models;
using System;

namespace SalaryUpdate.ViewModels
{
    public class SalaryUpdateViewModel
    {
        public Employer Employer { get; set; }
        public Employee Employee { get; set; }
        public ReturnedSalary ReturnedSalary { get; set; }

        public decimal RunningTotal => Math.Round(ReturnedSalary?.NewBasicCash ?? 0) +
                                       Math.Round(ReturnedSalary?.NewMealsProvided ?? 0) +
                                       Math.Round(ReturnedSalary?.NewCashMealAllowance ?? 0) +
                                       Math.Round(ReturnedSalary?.NewCashCellphoneAllowance ?? 0);

        public int TabIndex { get; set; }

        public bool ReadOnly { get; set; }
        public bool MultiEmployer => Employer?.MultiEmployer ?? false;
        public string LastSuccess { get; set; }
        public string Status { get; set; }
        public bool StatusPass { get; set; }

        public bool AllowSearch { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowCsvButton { get; set; }

        public SalaryUpdateViewModel()
        {
            LastSuccess = "N/A";
            Status = "Ready";
            StatusPass = true;
            TabIndex = (int)ViewElement.Employer;
        }
    }
}