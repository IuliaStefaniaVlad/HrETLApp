using HrappServices.Interfaces;
using HrappRepositories.Interfaces;
using HrappModels;
using Microsoft.Extensions.Logging;

namespace HrappServices
{
    public class JobStatusService : IJobStatusService
    {
        
        private readonly IRepository<JobStatusModel> _repository;
        private readonly ILogger<JobStatusService> _logger;

        public JobStatusService( IRepository<JobStatusModel> repository, ILogger<JobStatusService> logger) 
        { 
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException( nameof(logger));
        }

        public JobStatusModel GetJobStatus(Guid messageId)
        {
            try
            {
                var status = _repository.GetById(messageId);
                return status;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            
        }

        public void SetJobStatus(string messageId, string tenantId, string textMessage, bool success)
        {
            try
            {
                var jobStatusModel = new JobStatusModel()
                {
                    MessageID = messageId,
                    TenantID = tenantId,
                    TextMessage = textMessage,
                    Success = success
                };
                _repository.Add(jobStatusModel);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}