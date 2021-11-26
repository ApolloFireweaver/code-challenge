using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using challenge.Repositories;

namespace challenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICompensationRepository _compensationRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository, ICompensationRepository compensationRepository)
        {
            _employeeRepository = employeeRepository;
            _compensationRepository = compensationRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        public ReportingStructure FetchReportingStructure(string EmployeeId)
        {
            if (!String.IsNullOrEmpty(EmployeeId))
            {
                Employee targetEmployee = GetById(EmployeeId);
                //Check to make sure the employee we have exists and is filled out
                if(targetEmployee != null || targetEmployee.DirectReports == null)
                {
                    return null;
                }
                ReportingStructure employeeReportingStructure = new ReportingStructure();
                employeeReportingStructure.employee = targetEmployee;
                employeeReportingStructure.numberOfReports = numberOfReports(targetEmployee);

                return employeeReportingStructure;
            }

            return null;
        }

        //Despite the directions stating direct reports (the people who report to the target employee), the example suggests that you are looking for all reports
        //To that end, this will interate all the way down the reporting structure 
        private int numberOfReports(Employee employee)
        {
            //If you truely want the direct reports, and not the full strucuture, uncomment this line
            //return employee.directReports.Count();

            int numberOfDirectReports = 0;
            if (employee != null || employee.DirectReports == null)
            {
                return 0;
            }
            foreach(Employee underling in employee.DirectReports)
            {
                numberOfDirectReports++;
                numberOfDirectReports += numberOfReports(underling);
            }
            return numberOfDirectReports;
        }

        public Compensation CreateCompensation(Compensation compensation)
        {
            if(compensation != null)
            {
                _compensationRepository.Add(compensation);
                _compensationRepository.SaveAsync().Wait();
                return compensation;
            }
            return null;
        }
        public Compensation FetchCompensationForEmployee(string EmployeeId)
        {
            if (!String.IsNullOrEmpty(EmployeeId))
            {
                return _compensationRepository.GetById(EmployeeId);
            }

            return null;
        }
    }
}
