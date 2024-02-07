
namespace HrappModels
{
    public record ServiceBusQueueMessageModel
    {
        public string FileName { get; set; }
        public string TenantId { get; set; }

        public ServiceBusQueueMessageModel(string fileName, string tenantId) {
            FileName = fileName;
            TenantId = tenantId;
        }

    }
}
