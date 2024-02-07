using Azure.Storage.Blobs;
using HrappServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HrappServices
{
    public class UploadDataService : IUploadDataService
    {
        private readonly BlobContainerClient _container;
        private readonly ILogger<UploadDataService> _logger;

        public UploadDataService(BlobContainerClient container, ILogger<UploadDataService> logger)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> UploadFileToBlob(IFormFile fileForm, string tenantId)
        {
            if (fileForm == null) { return false; }
            if (string.IsNullOrWhiteSpace(fileForm.FileName)) { return false; }
            if (string.IsNullOrWhiteSpace(tenantId)) { return false; }
            var stream = new MemoryStream();
            try
            {

                string fileName = $"{Path.GetFileName(fileForm.FileName).Split(".").First()}_{tenantId}.csv";
                fileForm.CopyTo(stream);
                stream.Position = 0;

                //upload to Blob
                BlobClient blobClient = _container.GetBlobClient(fileName);
                var result = await blobClient.UploadAsync(stream, true);
                if (result.GetRawResponse().Status != 201)
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                stream.Dispose();
            }
            return true;
        }
    }
}
