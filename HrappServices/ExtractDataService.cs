using Azure.Storage.Blobs;
using CsvHelper.Configuration;
using CsvHelper;
using HrappModels;
using HrappServices.Interfaces;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace HrappServices
{
    public class ExtractDataService : IExtractDataService
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ILogger<ExtractDataService> _logger;

        public ExtractDataService(BlobContainerClient blobContainerClient, ILogger<ExtractDataService> logger)
        {
            _blobContainerClient = blobContainerClient ?? throw new ArgumentNullException(nameof(blobContainerClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<EmployeeRawDataModel>> ExtractDataAsync(string blobName)
        {
            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobName);
                var blobDataStream = blobClient.OpenRead();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };
                List<EmployeeRawDataModel> extractedData = new List<EmployeeRawDataModel>();

                var streamReader = new StreamReader(blobDataStream);
                var csv = new CsvReader(streamReader, config);
                extractedData =  await csv.GetRecordsAsync<EmployeeRawDataModel>().ToListAsync();
                if(extractedData.Any(e =>  e.GrossAnnualSalary < 0 ))
                {
                    throw new InvalidDataException("Invalid GrossAnnualSalary");
                }
                return extractedData;
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
