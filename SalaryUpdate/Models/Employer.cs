using System;
using System.ComponentModel.DataAnnotations;

namespace SalaryUpdate.Models
{
    public class Employer
    {
        [Required]
        public int EmployerId { get; set; }
        public string Name { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AltValue { get; set; }

        public bool MultiEmployer => AltValue != null && AltValue.Trim().Length > 0;
    }
}