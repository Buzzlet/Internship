using SalaryUpdate.Models;
using SalaryUpdate.Persistance;
using SalaryUpdate.Services;
using SalaryUpdate.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace SalaryUpdate.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataAccess _data = new DataAccess();

        public ActionResult Index(SalaryUpdateViewModel viewModel = null, string lastSuccess = "N/A")
        {
            var view = new SalaryUpdateViewModel();

            // if we're redirecting here from save action use that viewModel instead...
            if (viewModel?.Employee != null) view = viewModel;

            if (lastSuccess != "N/A") view.LastSuccess = lastSuccess;

            view.ReadOnly = true;
            view.AllowSearch = Security.CanSearchRestricted(User.Identity.Name);
            view.AllowUpdate = Security.CanUpdateRestricted(User.Identity.Name);
            view.AllowCsvButton = Security.CanExport(User.Identity.Name);

            return View("Index", view);
        }

        #region Search

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Search(SalaryUpdateViewModel viewModel)
        {
            viewModel.AllowCsvButton = Security.CanExport(User.Identity.Name);

            var employeeSuccess = GetEmployee(viewModel);
            if (!employeeSuccess)
            {
                ModelState.AddModelError("Employee.EmployeeId", "No employee record found! Make sure you entered the Employee number correctly.");
                viewModel.ReadOnly = true;

                return View("Index", viewModel);
            }

            var employerSuccess = GetEmployer(viewModel);
            if (!employerSuccess)
            {
                ModelState.AddModelError("EmployerId.EmployerId", "No employer record found! Make sure you entered the EmployerId number correctly.");
                viewModel.ReadOnly = true;
            }
            else
            {
                if (viewModel.Employee.EmployerId != viewModel.Employer.EmployerId) // check if employee still works at employer
                {
                    ModelState.AddModelError("Employee.EmployeeId", "Employee does not work at " + viewModel.Employer.Name + ".");
                    viewModel.ReadOnly = true;
                }
                else
                {
                    if (!Security.CanViewRight(viewModel.Employer.EmployerId, User.Identity.Name))
                    {
                        ModelState.AddModelError("Employee.EmployeeId", "The employee is restricted and you do not have sufficient rights to view.");
                        viewModel.AllowSearch = false;
                    }
                    else
                    {
                        viewModel.AllowSearch = true;
                        viewModel.TabIndex = (int)ViewElement.Hours;
                    }

                    viewModel.ReadOnly = !Security.CanUpdateRight(viewModel.Employer.EmployerId, User.Identity.Name);

                    if (!Security.CanUpdateEmployee(viewModel.Employee.Status))
                    {
                        ModelState.AddModelError("Employee.EmployeeId", "Employee status forbids salary change!");
                        viewModel.ReadOnly = true;
                    }
                }
            }

            return View("Index", viewModel);
        }

        private bool GetEmployee(SalaryUpdateViewModel viewModel)
        {
            var matchingEmployee = new Employee();
            if (viewModel.Employee != null)
                matchingEmployee = _data.GetEmployeeInfo(viewModel.Employee.EmployeeId);

            if (matchingEmployee == null || matchingEmployee.EmployeeId == 0)
            {
                viewModel.Employee = null;
                return false;
            }
            viewModel.Employee = matchingEmployee;
            viewModel.ReturnedSalary = _data.GetReturnedSalary(viewModel.Employee.EmployeeId.ToString()) ?? new ReturnedSalary();
            return true;
        }

        private bool GetEmployer(SalaryUpdateViewModel viewModel)
        {
            var matchingEmployer = new Employer();
            if (viewModel.Employer?.EmployerId != null)
            {
                matchingEmployer = _data.GetEmployerInfo(viewModel.Employer.EmployerId);
            }

            if (matchingEmployer.Name == null)
            {
                viewModel.Employer = null;
                return false;
            }

            viewModel.Employer = matchingEmployer;
            return true;
        }

        #endregion

        #region Save

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(SalaryUpdateViewModel viewModel)
        {
            RepopulateVm(viewModel);

            if (viewModel.RunningTotal != viewModel.ReturnedSalary.NewTotalSalary)
            {
                ModelState.AddModelError("ValidationSummary", "The Total Salary entered does not match the running total!");
                viewModel.ReadOnly = false;
            }

            if (ModelState.IsValid)
            {
                if (PassedEdits(viewModel))
                {
                    if (!Security.CanUpdateRight(viewModel.Employer.EmployerId, User.Identity.Name))
                    {
                        ModelState.AddModelError("ValidationSummary", "You do not have sufficient rights to update restricted employees!");
                        viewModel.TabIndex = (int)ViewElement.Employer;
                    }
                    else
                    {
                        SaveOrAddRecord(viewModel);
                        ModelState.Clear();
                        viewModel = new SalaryUpdateViewModel // a new model that only has fields we want to keep
                        {
                            StatusPass = viewModel.StatusPass,
                            Status = viewModel.Status,
                            LastSuccess = viewModel.LastSuccess,
                            Employer = viewModel.Employer,
                            AllowSearch = viewModel.AllowSearch,
                            AllowUpdate = viewModel.AllowUpdate,
                            AllowCsvButton = viewModel.AllowCsvButton,
                            TabIndex = (int)ViewElement.Employee
                        };

                        // we want to clear all fields but employer's EmployerId / LastSuccess and make the page readonly
                    }
                }
                else
                {
                    viewModel.Status = "Zero records added";
                    viewModel.StatusPass = false;
                }
            }
            else
            {
                viewModel.Status = "Zero records added";
                viewModel.StatusPass = false;

                if (!viewModel.ReadOnly)
                {
                    viewModel.TabIndex = (int)ViewElement.Hours;
                }
            }

            return View("Index", viewModel);
        }

        private void RepopulateVm(SalaryUpdateViewModel viewModel)
        {
            // Give me back my values, dangit!
            if (viewModel.Employee != null)
            {
                var newHoursPerWeek = viewModel.ReturnedSalary.NewHoursPerWeek;
                var newBasicCash = viewModel.ReturnedSalary.NewBasicCash;
                var newMealsProvided = viewModel.ReturnedSalary.NewMealsProvided;
                var newCashMealAllowance = viewModel.ReturnedSalary.NewCashMealAllowance;
                var newCashCellphoneAllowance = viewModel.ReturnedSalary.NewCashCellphoneAllowance;
                var newTotalSalary = viewModel.ReturnedSalary.NewTotalSalary;

                viewModel.Employee = _data.GetEmployeeInfo(viewModel.Employee.EmployeeId);
                viewModel.ReturnedSalary.NewHoursPerWeek = newHoursPerWeek;
                viewModel.ReturnedSalary.NewBasicCash = newBasicCash;
                viewModel.ReturnedSalary.NewMealsProvided = newMealsProvided;
                viewModel.ReturnedSalary.NewCashMealAllowance = newCashMealAllowance;
                viewModel.ReturnedSalary.NewCashCellphoneAllowance = newCashCellphoneAllowance;
                viewModel.ReturnedSalary.NewTotalSalary = newTotalSalary;
                viewModel.ReturnedSalary.EmployeeId = viewModel.Employee.EmployeeId.ToString();
            }
            if (viewModel.Employer != null)
            {
                viewModel.ReturnedSalary.EmployerId = viewModel.Employer.EmployerId;

                if (viewModel.Employee != null)
                    viewModel.Employee.EmployerId = viewModel.Employer.EmployerId;

                viewModel.Employer = _data.GetEmployerInfo(viewModel.Employer.EmployerId);

                viewModel.ReadOnly = !Security.CanUpdateRight(viewModel.Employer.EmployerId, User.Identity.Name);
            }

            viewModel.AllowSearch = Security.CanSearchRestricted(User.Identity.Name);
            viewModel.AllowUpdate = Security.CanUpdateRestricted(User.Identity.Name);
            viewModel.AllowCsvButton = Security.CanExport(User.Identity.Name);
        }

        private bool PassedEdits(SalaryUpdateViewModel viewModel)
        {
            // Employee Status check
            if (!Security.EmployeeCodes.Contains("~" + viewModel.Employee.Status))
            {
                ModelState.AddModelError("ValidationSummary", "The employee is in a status where the salary cannot be changed!");
                viewModel.TabIndex = (int)ViewElement.Employee;
                viewModel.ReadOnly = true;
            }
            // Meal Allowance Provided must be 5% even if overriden unless multi employer
            var meals = (viewModel.ReturnedSalary.NewBasicCash * new decimal(0.05)).RoundToDollars();
            if (viewModel.ReturnedSalary.NewMealsProvided != meals && viewModel.ReturnedSalary.NewMealsProvided > 0)
            {
                if (!viewModel.MultiEmployer)
                {
                    ModelState.AddModelError("ValidationSummary", "The employer is not a multiple employer! The Meal Allowance Provided must be equal to 5% of the Cash Salary.");
                    viewModel.TabIndex = (int)ViewElement.MealAllowanceProvided;
                }
            }

            // Check for already existing record in EmployeeActions 
            if (_data.HasSalaryAlready(viewModel.Employee.EmployeeId))
            {
                ModelState.AddModelError("ValidationSummary", "This person already has a salary keyed for " + ConfigurationManager.AppSettings.Get("EffectiveDate"));
                viewModel.TabIndex = (int)ViewElement.Employee;
            }

            // If after all that we don't have any errors, ayyyy
            if (ModelState.IsValid)
                return true;

            // If the employee is restriced, see if user can update
            if (!ModelState.Values.SelectMany(modelState => modelState.Errors)
                .Any(error => error.ErrorMessage == "The employee is restricted and you do not have sufficient rights to view." &&
                              Security.CanUpdateRestricted(User.Identity.Name))) return false;

            viewModel.TabIndex = (int)ViewElement.Employer;
            return true;
        }

        private void SaveOrAddRecord(SalaryUpdateViewModel viewModel)
        {
            var returnedSalary = viewModel.ReturnedSalary;
            returnedSalary.Exported = false;

            if (!_data.UpdateReturnedSalary(returnedSalary))
            {
                ModelState.AddModelError("ValidationSummary", "This employee already has more than 1 record!");
                viewModel.Status = "Zero records added";
                viewModel.StatusPass = false;
            }
            else
            {
                viewModel.Status = "One record added";
                viewModel.StatusPass = true;
                viewModel.LastSuccess = viewModel.Employee.EmployeeId.ToString("00000");
            }
        }

        #endregion

        #region Export

        public JsonResult MakeCsv()
        {
            try
            {
                if (!Security.CanExport(User.Identity.Name)) return Json(new { error = true, message = "You do not have permission for this action." });

                using (var export = new ExportService(_data))
                {
                    var success = export.Export();
                    return Json(new { error = !success, message = success ? "Export Successfull" : "Export Failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            _data?.Dispose();
            base.Dispose(disposing);
        }
    }
}