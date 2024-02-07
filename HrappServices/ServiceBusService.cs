using HrappModels;
using HrappServices.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace HrappServices
{
    public class ServiceBusService : IServiceBusService
    {
        
        private readonly IQueueClient _client;
        private readonly ILogger<ServiceBusService> _logger;

        public ServiceBusService(IQueueClient client, ILogger<ServiceBusService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendMessageToServiceBusAsync(string fileName, string tenantId, string messageId)
        {
            try 
            { 
                var messageBody = JsonSerializer.Serialize(new ServiceBusQueueMessageModel($"{Path.GetFileName(fileName).Split(".").First()}_{tenantId}.csv", tenantId));
                //Set content type and Guid
                var message = new Message(Encoding.UTF8.GetBytes(messageBody))
                {
                    MessageId = messageId,
                    ContentType = "application/json"
                };
            
                await _client.SendAsync(message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            return true;
        }
    }
}
