using SalaryUpdate.Models;
using SalaryUpdate.Persistance.Configurations;
using System.Data.Entity;

namespace SalaryUpdate.Persistance
{
    public class SalaryUpdateContext : DbContext
    {
        public DbSet<ReturnedSalary> ReturnedSalaries { get; set; }

        public SalaryUpdateContext() : base("DefaultConnection") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ReturnedSalaryConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}