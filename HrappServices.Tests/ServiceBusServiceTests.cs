using HrappModels;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HrappServices.Tests
{
    public class ServiceBusServiceTests
    {
        private Mock<ILogger<ServiceBusService>> _mockLogger = new Mock<ILogger<ServiceBusService>>();
        private Mock<IQueueClient> _mockClient = new Mock<IQueueClient>();
        [Fact]
        public async void SendMessageToServiceBusAsync_OK()
        {
            //Arrange
            string fileName = "Test.csv";
            string messageId = Guid.NewGuid().ToString();
            string tenantId = Guid.NewGuid().ToString();
            
           
            _mockClient.Setup(x => x.SendAsync(It.IsAny<Message>()))
                      .Verifiable();

            var service = new ServiceBusService(_mockClient.Object, _mockLogger.Object);

            //Act
            var success = await service.SendMessageToServiceBusAsync(fileName, messageId, tenantId);

            //Assert
            Assert.True(success);
            _mockClient.Verify(x => x.SendAsync(It.IsAny<Message>()), Times.Once());

        }

        [Fact]
        public async void SendMessageToServiceBusAsync_Fail()
        {
            string fileName = "Test.csv";
            string messageId = Guid.NewGuid().ToString();
            string tenantId = Guid.NewGuid().ToString();
            var exception = new Exception("Could not send message to ServiceBus");

            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), exception, It.IsAny<Func<object, Exception?, string>>()))
                      .Verifiable();
            _mockClient.Setup(x => x.SendAsync(It.IsAny<Message>()))
                      .Throws(exception)
                      .Verifiable();

            var service = new ServiceBusService(_mockClient.Object, _mockLogger.Object);

            //Act
            var success = await service.SendMessageToServiceBusAsync(fileName, messageId, tenantId);

            //Assert
            Assert.False(success);
            _mockClient.Verify(x => x.SendAsync(It.IsAny<Message>()), Times.Once());
            _mockLogger.Verify(logger => logger.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.Is<EventId>(eventId => eventId.Id == 0),
                            It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == exception.Message && @type.Name == "FormattedLogValues"),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                                Times.Once());
        }
    }
}
