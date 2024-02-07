

using HrappModels;

namespace HrappServices.Interfaces
{
    public interface IJobStatusService
    {

        void SetJobStatus(string messageId, string tenantId, string textMessage, bool success);
        JobStatusModel GetJobStatus(Guid jobStatusId);
    }
}
