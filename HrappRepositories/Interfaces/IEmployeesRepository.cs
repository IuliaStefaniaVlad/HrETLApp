using HrappModels;
using System.Linq.Expressions;

namespace HrappRepositories.Interfaces
{
    public interface IEmployeesRepository : IRepository<EmployeeDBModel>
    {
        EmployeeDBModel? GetEmployee(int employeeId, string tenantId);
    }
}
