using SalaryUpdate.Models;
using SalaryUpdate.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SalaryUpdate.Persistance
{
    public class DataAccess : IDisposable
    {
        private readonly SourceContext _sourceContext;
        private readonly SalaryUpdateContext _salaryContext;

        private const string BackupProcedure = "BackupReturnedSalariesBeforeExport";

        public DataAccess()
        {
            _sourceContext = new SourceContext();
            _salaryContext = new SalaryUpdateContext();
        }

        public Employee GetEmployeeInfo(int employeeId)
        {
            var employeeInfo = (from e in _sourceContext.Employees
                join s in _sourceContext.Systems on new { e.LocationId, e.EmployerId } equals new
                {
                    s.LocationId,
                    s.EmployerId
                }
                join ee in _sourceContext.EmployeesExtended on new { e.EmployeeId, e.LocationId } equals new
                {
                    ee.EmployeeId,
                    ee.LocationId
                }
                where e.EmployeeId == employeeId
                select
                    new Employee
                    {
                        EmployeeId = e.EmployeeId,
                        EmployerName = s.Name,
                        EmployerId = e.EmployerId,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Status = e.Status,
                        Rate = e.Rate,
                        Hours = e.Hours,
                        CashSalary = ee.Salary1,
                        MealAllowanceProvided = ee.Salary2,
                        CashMealAllowance = ee.Salary3,
                        CashCellphoneAllowance = ee.Salary4,
                    }).SingleOrDefault();

            return employeeInfo;
        }

        public ReturnedSalary GetReturnedSalary(string employeeId)
        {
            return _salaryContext.ReturnedSalaries.SingleOrDefault(s => s.EmployeeId == employeeId);
        }

        public Employer GetEmployerInfo(int employerId)
        {
            var general = _sourceContext.Systems
                .Where(s => s.EmployerId == employerId)
                .Select(s => new { s.Name, s.EmployerId })
                .SingleOrDefault();

            var multiEmployer = (from a in _sourceContext.Positions
                join b in _sourceContext.CustomFields on new { Comp = a.LocationId, ObjId = a.ObjectId } equals new
                {
                    Comp = b.LocationId,
                    ObjId = b.ObjectId
                }
                where b.Type == "0" &&
                      b.Key == "Multi" &&
                      a.Value == employerId.ToString() &&
                      b.EffectiveDate < DateTime.Now &&
                      (b.EndDate >= DateTime.Now || b.EndDate == null)
                select new
                {
                    b.EffectiveDate,
                    b.EndDate,
                    b.AltValue
                }).Distinct().SingleOrDefault();

            return new Employer
            {
                Name = general?.Name,
                EmployerId = general?.EmployerId ?? 0, // added nullable default to 0 - 2/26 KSH

                EffectiveDate = multiEmployer?.EffectiveDate ?? new DateTime(),
                EndDate = multiEmployer?.EndDate ?? new DateTime(),
                AltValue = multiEmployer?.AltValue
            };
        }

        /// <summary>
        /// Saves a backup of the db and returns a list of ReturnedSalaries that have not been exported 
        /// </summary>
        public List<ReturnedSalary> GetExportDataForSalaryUpdate()
        {
            _salaryContext.Database.ExecuteSqlCommand(BackupProcedure); // backup the table first
            return _salaryContext.ReturnedSalaries.Where(s => s.Exported == false).ToList();
        }

        // Update an existing ReturnedSalary or make a new record Returns true or false based on success
        public bool UpdateReturnedSalary(ReturnedSalary newData)
        {
            var updated = false;

            // get the corresponding record
            var employeeSalaries = _salaryContext.ReturnedSalaries
                .Where(s => string.Compare(s.EmployeeId, newData.EmployeeId, StringComparison.Ordinal) == 0)
                .ToList();

            if (employeeSalaries.Count == 0)
            {
                newData.DateCreated = DateTime.Now;
                newData.DateModified = DateTime.Now;
                _salaryContext.ReturnedSalaries.Add(newData);
                _salaryContext.SaveChanges();
                updated = true;
            }
            else if (employeeSalaries.Count == 1)
            {
                employeeSalaries[0].DateCreated = newData.DateCreated;
                employeeSalaries[0].DateModified = DateTime.Now;
                employeeSalaries[0].EmployerId = newData.EmployerId;
                employeeSalaries[0].Exported = newData.Exported;
                employeeSalaries[0].NewBasicCash = newData.NewBasicCash;
                employeeSalaries[0].NewCashMealAllowance = newData.NewCashMealAllowance;
                employeeSalaries[0].NewCashCellphoneAllowance = newData.NewCashCellphoneAllowance;
                employeeSalaries[0].NewHoursPerWeek = newData.NewHoursPerWeek;
                employeeSalaries[0].NewMealsProvided = newData.NewMealsProvided;
                employeeSalaries[0].NewTotalSalary = newData.NewTotalSalary;

                _salaryContext.SaveChanges();
                updated = true;
            }
            // If we haven't met either condition, we're not sure which one to update, so let's do nothing
            return updated;
        }

        // Sets the Exported flag to true for all each record passed in (this was really poorly documented)
        // Returns true or false based on success
        public bool UpdateExportedSalaryRecords(List<string> employeeIds)
        {
            if (employeeIds == null) throw new ArgumentNullException(nameof(employeeIds));

            var updated = true;
            foreach (var employeeId in employeeIds)
            {
                var result = _salaryContext.ReturnedSalaries.SingleOrDefault(s => s.EmployeeId == employeeId);
                if (result == null) // we weren't able to update this one!
                {
                    updated = false;
                }
                else
                {
                    result.Exported = true;
                    result.DateExported = DateTime.Now;
                }
            }
            _salaryContext.SaveChanges();

            return updated;
        }

        // Determines if the employee has updated salary already
        public bool HasSalaryAlready(int employeeId)
        {
            var effectiveDate = DateTime.Parse(Settings.Default.EffectiveDate);

            var count = _sourceContext.EmployeeActions.Where(s =>
                new
                {
                    s.LocationId,
                    Code = s.Action,
                    s.EffectiveDate,
                    s.EmployeeId
                } ==
                new
                {
                    LocationId = (short)583, 
                    Code = "SALUPD",
                    EffectiveDate = effectiveDate,
                    EmployeeId = employeeId
                }).ToList().Count;

            return count > 0;
        }

        public void Dispose()
        {
            //_sourceContext?.Dispose();
            _salaryContext?.Dispose();
        }
    }
}