using HrappModels;

namespace HrappServices.Interfaces
{
    public interface ITransformDataService
    {
        List<EmployeeDBModel> TransformData(IEnumerable<EmployeeRawDataModel> rawData, string tenantId);
    }
}
