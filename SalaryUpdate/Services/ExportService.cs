using System;
using System.Collections.Generic;
using System.IO;
using SalaryUpdate.Models;
using SalaryUpdate.Persistance;
using SalaryUpdate.Properties;

namespace SalaryUpdate.Services
{
    public class ExportService : IDisposable
    {
        private readonly DataAccess _data;

        public ExportService(DataAccess data)
        {
            _data = data;
        }

        public bool Export()
        {
            var recordsToUpdate = _data.GetExportDataForSalaryUpdate();
            if (recordsToUpdate.Count == 0) throw new Exception("No data to Export");

            var path = Settings.Default.ExportFilePath;
            SaveBackUpFile(path);
            return WriteRecordsToFile(recordsToUpdate, path);
        }

        private static void SaveBackUpFile(string path)
        {
            if (!File.Exists(path)) return;

            var newPath = path + "_" + DateTime.Now.ToString("yyyyMMdd");
            if (File.Exists(newPath)) File.Delete(newPath);

            using (var sw = new StreamWriter(File.OpenWrite(newPath)))
            {
                using (var sr = new StreamReader(File.OpenRead(path)))
                {
                    sw.Write(sr.ReadToEnd()); // read from old file and write to new file
                }
            }
        }

        private bool WriteRecordsToFile(IEnumerable<ReturnedSalary> records, string path)
        {
            if (File.Exists(path)) File.Delete(path);

            bool success;
            using (var streamWriter = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
            {
                var employeesToUpdate = new List<string>();

                foreach (var returnedSalary in records)
                {
                    var output = returnedSalary.EmployeeId + ","
                                                             + returnedSalary.NewTotalSalary.RoundToDollars() + ","
                                                             + HandleBasicCash(returnedSalary.NewBasicCash) + ","
                                                             + returnedSalary.NewMealsProvided.RoundToDollars() + ","
                                                             + returnedSalary.NewCashMealAllowance.RoundToDollars() + ","
                                                             + returnedSalary.NewCashCellphoneAllowance.RoundToDollars() + ","
                                                             + returnedSalary.NewHoursPerWeek;

                    streamWriter.WriteLine(output);

                    employeesToUpdate.Add(returnedSalary.EmployeeId);
                }

                success = _data.UpdateExportedSalaryRecords(employeesToUpdate);
                streamWriter.Close();
            }
            return success;
        }

        private static decimal HandleBasicCash(decimal basicCash)
        {
            return basicCash == 0.01M || basicCash == 0 ? 0.01M : basicCash.RoundToDollars();
        }

        public void Dispose()
        {
            _data?.Dispose();
        }
    }
}