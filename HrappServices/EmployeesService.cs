using HrappModels;
using HrappRepositories.Interfaces;
using HrappServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace HrappServices
{
    public class EmployeesService : IEmployeesService
    {
        private readonly IEmployeesRepository _employeeRepository;
        private readonly ILogger<EmployeesService> _logger; 

        public EmployeesService(IEmployeesRepository employeesRepository, ILogger<EmployeesService> logger)
        {
            _employeeRepository = employeesRepository ?? throw new ArgumentNullException(nameof(employeesRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddEmployees(List<EmployeeDBModel> employees)
        {
            try
            {
                _employeeRepository.AddRange(employees);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public EmployeeModel? GetEmployee(int employeeId, string tenantId)
        {
            try
            {
                var employee = _employeeRepository.GetEmployee(employeeId, tenantId);
                if (employee == null)
                {
                    return null;
                }
                var returnEmployee = new EmployeeModel()
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    BirthDate = employee.BirthDate,
                    AnnualIncome = employee.AnnualIncome,
                };
                return returnEmployee;
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
