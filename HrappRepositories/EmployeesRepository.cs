using HrappModels;
using HrappRepositories.DBContext;
using HrappRepositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace HrappRepositories
{
    public class EmployeesRepository : Repository<EmployeeDBModel>, IEmployeesRepository
    {
        public EmployeesRepository(EmployeeDbContext employeeDbContext) : base(employeeDbContext)
        {}
        private EmployeeDbContext? EmployeeDbContext
        {
            get { return _dbContext as EmployeeDbContext; }
        }
        public EmployeeDBModel? GetEmployee(int employeeId, string tenantId)
        {
            var employee = EmployeeDbContext?.Employee.Single(e => e.EmployeeID == employeeId &&  e.TenantID == tenantId);
            
            if (employee == null)
            {
                return null;
            }
            
            return employee;
        }

        public new EmployeeModel? Find(Expression<Func<EmployeeDBModel,bool>> predicate)
        {
            var employee = EmployeeDbContext?.Employee.Single(predicate);

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
    }
}
