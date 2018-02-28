using SalaryUpdate.Models;
using System.Data.Entity.ModelConfiguration;

namespace SalaryUpdate.Persistance.Configurations
{
    public class ReturnedSalaryConfiguration : EntityTypeConfiguration<ReturnedSalary>
    {
        public ReturnedSalaryConfiguration()
        {
            HasKey(s => s.EmployeeId);

            Property(s => s.EmployeeId).HasMaxLength(10).IsRequired();
            Property(s => s.EmployerId).IsRequired();

            Property(s => s.NewTotalSalary);
            Property(s => s.NewBasicCash);
            Property(s => s.NewMealsProvided);
            Property(s => s.NewCashMealAllowance);
            Property(s => s.NewCashCellphoneAllowance);
            Property(s => s.NewHoursPerWeek);
            Property(s => s.Exported);
            Property(s => s.DateCreated);
            Property(s => s.DateModified);
            Property(s => s.DateExported);
        }
    }
}