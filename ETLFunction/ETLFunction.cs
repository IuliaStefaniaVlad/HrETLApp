using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using HrappModels;
using HrappServices.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ETLFunction
{
    public class ETLFunction
    {
        private readonly ILogger<ETLFunction> _logger;
        private readonly IExtractDataService _extractDataService;
        private readonly ITransformDataService _transformDataService;
        private readonly IEmployeesService _employeesService;
        private readonly IJobStatusService _jobStatusService;

        public ETLFunction(ILogger<ETLFunction> logger, IExtractDataService extractDataService, 
                                                        ITransformDataService transformDataService,
                                                        IEmployeesService employeesService,
                                                        IJobStatusService jobStatusService)
        {
            _logger = logger;
            _extractDataService = extractDataService ?? throw new ArgumentNullException(nameof(extractDataService));
            _transformDataService = transformDataService ?? throw new ArgumentNullException(nameof(transformDataService));
            _employeesService = employeesService ?? throw new ArgumentNullException(nameof(employeesService));
            _jobStatusService = jobStatusService ?? throw new ArgumentNullException(nameof(jobStatusService));
        }


        [Function(nameof(ETLFunction))]
        public async Task Run(
            [ServiceBusTrigger("hrappqueue", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            var details = message.Body.ToObjectFromJson<ServiceBusQueueMessageModel>();
            var rawData = await _extractDataService.ExtractDataAsync(details.FileName);
            if(rawData == null || rawData.Count == 0)
            {
                _jobStatusService.SetJobStatus(message.MessageId, details.TenantId, "Upload Failed.", false);
                return;
            }
            var transformedData = _transformDataService.TransformData(rawData, details.TenantId);
            if(transformedData == null || transformedData.Count == 0)
            {
                _jobStatusService.SetJobStatus(message.MessageId, details.TenantId, "Upload Failed.", false);
                return;
            }
            try
            {
                _employeesService.AddEmployees(transformedData);
            }
            catch
            {
                _jobStatusService.SetJobStatus(message.MessageId, details.TenantId, "Upload Failed.", false);
                return;
            }
            
            //Put Message in table
            _jobStatusService.SetJobStatus(message.MessageId, details.TenantId, "Upload Finished.", true);
            
            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
