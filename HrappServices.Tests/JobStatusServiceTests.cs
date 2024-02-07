using HrappModels;
using HrappRepositories.Interfaces;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrappServices.Tests
{

    public class JobStatusServiceTests
    {
        private Mock<IRepository<JobStatusModel>> _mockRepository = new Mock<IRepository<JobStatusModel>>();
        private Mock<ILogger<JobStatusService>> _mockLogger = new Mock<ILogger<JobStatusService>>();


        [Fact]
        public void GetJobStatus_OK()
        {
            //Arrange
            var messageId = Guid.NewGuid();
            var jobStatus = new JobStatusModel()
            {
                MessageID = messageId.ToString(),
                TenantID = Guid.NewGuid().ToString(),
                TextMessage = "Test",
                Success = true
            };
            _mockRepository.Setup(x => x.GetById(messageId))
                .Returns(jobStatus)
                .Verifiable();

            var service = new JobStatusService(_mockRepository.Object, _mockLogger.Object);

            //Act
            var result = service.GetJobStatus(messageId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(jobStatus, result);

            _mockRepository.Verify(s => s.GetById(It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public void GetJobStatus_Fail()
        {
            //Arrange
            var messageId = Guid.NewGuid();
            var exception = new Exception("Could not get status.");
            _mockRepository.Setup(x => x.GetById(messageId))
                .Throws(exception)
                .Verifiable();
            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), exception, It.IsAny<Func<object, Exception?, string>>())).Verifiable();

            var service = new JobStatusService(_mockRepository.Object, _mockLogger.Object);

            //Act
            var result = service.GetJobStatus(messageId);

            //Assert
            _mockRepository.Verify(s => s.GetById(It.IsAny<Guid>()), Times.Once());
            _mockLogger.Verify(logger => logger.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.Is<EventId>(eventId => eventId.Id == 0),
                            It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == exception.Message && @type.Name == "FormattedLogValues"),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                                Times.Once);
        }

        [Fact]
        public void SetJobStatus_OK()
        {
            //Arrange
            string messageId = Guid.NewGuid().ToString();
            string tenantId = Guid.NewGuid().ToString();
            string textMessage = "Test";
            bool success = true;
            List<JobStatusModel> data = new();

            _mockRepository.Setup(x => x.Add(It.IsAny<JobStatusModel>()))
                .Callback((JobStatusModel x) => { data.Add(x);})
                .Verifiable();
            var service = new JobStatusService(_mockRepository.Object, _mockLogger.Object);

            //Act
            service.SetJobStatus(messageId, tenantId, textMessage, success);

            //Assert
            Assert.NotEmpty(data);
            Assert.Equal(messageId, data[0].MessageID);
            Assert.Equal(tenantId, data[0].TenantID);
            Assert.Equal(textMessage, data[0].TextMessage);
            Assert.True(data[0].Success);
            _mockRepository.Verify(s => s.Add(It.IsAny<JobStatusModel>()), Times.Once());

        }

        [Fact]
        public void SetJobStatus_Fail()
        {
            //Arrange
            string messageId = Guid.NewGuid().ToString();
            string tenantId = Guid.NewGuid().ToString();
            string textMessage = "Test";
            bool success = true;
            var exception = new Exception("Failed");
            List<JobStatusModel> data = new();

            _mockRepository.Setup(x => x.Add(It.IsAny<JobStatusModel>()))
                .Throws(exception)
                .Verifiable();
            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), exception, It.IsAny<Func<object, Exception?, string>>())).Verifiable();
            var service = new JobStatusService(_mockRepository.Object, _mockLogger.Object);

            //Act
            service.SetJobStatus(messageId, tenantId, textMessage, success);

            //Assert
            _mockRepository.Verify(s => s.Add(It.IsAny<JobStatusModel>()), Times.Once());
            _mockLogger.Verify(logger => logger.Log(
                           It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                           It.Is<EventId>(eventId => eventId.Id == 0),
                           It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == exception.Message && @type.Name == "FormattedLogValues"),
                           It.IsAny<Exception>(),
                           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                               Times.Once);

        }
    }
}
