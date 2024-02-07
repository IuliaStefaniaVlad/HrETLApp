using Azure.Storage.Blobs;
using HrappModels;

namespace HrappServices.Interfaces
{
    public interface IExtractDataService
    {
        Task<List<EmployeeRawDataModel>> ExtractDataAsync(string blobName);
    }
}
