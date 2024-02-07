using Microsoft.AspNetCore.Http;

namespace HrappServices.Interfaces
{
    public interface IUploadDataService
    {
        Task<bool> UploadFileToBlob(IFormFile fileForm, string tenantId);
    }
}
