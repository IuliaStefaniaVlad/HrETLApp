using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrappServices.Interfaces
{
    public interface IServiceBusService
    {
        Task<bool> SendMessageToServiceBusAsync(string fileName, string tenantId, string messageId);
    }
}
