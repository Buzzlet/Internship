using SalaryUpdate.Properties;
using System.Collections.Generic;
using System.Linq;

namespace SalaryUpdate.Services
{
    public static class Security
    {
        public static IEnumerable<string> RestrictedEmployers => new[] {"1234", "4567", "8794"};
        public static IEnumerable<string> EmployeeCodes => new[] {"A", "B", "C", "D", "E", "F"};

        public static bool CanSearchRestricted(string userName)
        {
            return Settings.Default.RestrictedRead.Split('|').Contains(userName.ToUpper());
        }

        public static bool CanUpdateRestricted(string userName)
        {
            return Settings.Default.RestrictedWrite.Split('|').Contains(userName.ToUpper());
        }

        public static bool CanExport(string userName)
        {
            return Settings.Default.CsvBtnAllowed.Split('|').Contains(userName.ToUpper());
        }

        public static bool CanUpdateEmployee(string employeeStatus)
        {
            return EmployeeCodes.Contains(employeeStatus);
        }

        public static bool CanUpdateRight(int employerId, string username)
        {
            return !RestrictedEmployers.Contains(employerId.ToString()) || CanUpdateRestricted(username);
        }

        public static bool CanViewRight(int employerId, string username)
        {
            return !RestrictedEmployers.Contains(employerId.ToString()) || CanSearchRestricted(username);
        }
    }
}