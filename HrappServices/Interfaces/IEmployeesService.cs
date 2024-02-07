using HrappModels;

namespace HrappServices.Interfaces
{
    public interface IEmployeesService
    {
        void AddEmployees(List<EmployeeDBModel> employees);
        EmployeeModel? GetEmployee(int epmployeeId, string tenantId);
    }
}
