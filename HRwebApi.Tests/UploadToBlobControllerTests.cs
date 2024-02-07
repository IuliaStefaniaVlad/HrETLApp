using HrappServices.Interfaces;
using HrappServices;
using HRwebApi.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HrappModels;

namespace HrappControllers.Tests
{
    public class UploadToBlobControllerTests
    {
        private Mock<IUploadDataService> _mockUploadDataService = new Mock<IUploadDataService>();
        private Mock<IServiceBusService> _mockServiceBusService = new Mock<IServiceBusService>();
        private Mock<IJobStatusService> _mockJobStatusService = new Mock<IJobStatusService>();
        private Mock<ILogger<UploadToBlobController>> _mockLogger = new Mock<ILogger<UploadToBlobController>>();

        [Fact]
        public async void UploadFileToBlob_OK()
        {
            //Arrange
            var messageId = Guid.NewGuid().ToString();
            var tenantId = Guid.NewGuid().ToString();
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile fileData = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            var fileModel = new FileModel()
            {
                FileData = fileData
            };
            
            _mockUploadDataService.Setup(x => x.UploadFileToBlob(fileData, tenantId))
                                  .ReturnsAsync(true)
                                  .Verifiable();

            _mockServiceBusService.Setup(x => x.SendMessageToServiceBusAsync(fileModel.FileData.FileName, tenantId, It.IsAny<string>()))
                                  .ReturnsAsync(true)
                                  .Verifiable();

            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            var fakeHttpContext = new Mock<HttpContext>();
            var claims = new List<Claim>()
            {
                new Claim("TenantId", tenantId),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            fakeHttpContext.Setup(t => t.User).Returns(claimsPrincipal);
            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Object.HttpContext = fakeHttpContext.Object;
            controller.ControllerContext = controllerContext.Object;

            //Act
            var result = await controller.UploadFileToBlob(fileModel);

            //Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
            Assert.Equal(200,okResult.StatusCode);
            _mockUploadDataService.Verify(x => x.UploadFileToBlob(fileData, tenantId), Times.Once());
            _mockServiceBusService.Verify(x => x.SendMessageToServiceBusAsync(fileModel.FileData.FileName, tenantId, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async void UploadFileToBlob_TenantNotFound()
        {
            //Arrange
            var tenantId = Guid.NewGuid().ToString();
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile fileData = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            var fileModel = new FileModel()
            {
                FileData = fileData
            };
            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception?, string>>())).Verifiable();

            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            //Act
            var result = await controller.UploadFileToBlob(fileModel);

            //Assert
            Assert.NotNull(result);
            var badResponse = Assert.IsType<ObjectResult>(result);
            Assert.Equal(badResponse.StatusCode, 500);
            _mockLogger.Verify(logger => logger.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.Is<EventId>(eventId => eventId.Id == 0),
                            It.Is<It.IsAnyType>((@object, @type) => @type.Name == "FormattedLogValues"),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                                Times.Once());
        }

        [Fact]
        public async void UploadFileToBlob_FailUpload()
        {
            //Arrange
            var tenantId = Guid.NewGuid().ToString();
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile fileData = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            var fileModel = new FileModel()
            {
                FileData = fileData
            };
            _mockUploadDataService.Setup(x => x.UploadFileToBlob(fileData, tenantId))
                                  .ReturnsAsync(false)
                                  .Verifiable();

            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            var fakeHttpContext = new Mock<HttpContext>();
            var claims = new List<Claim>()
            {
                new Claim("TenantId", tenantId),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            fakeHttpContext.Setup(t => t.User).Returns(claimsPrincipal);
            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Object.HttpContext = fakeHttpContext.Object;
            controller.ControllerContext = controllerContext.Object;

            //Act
            var result = await controller.UploadFileToBlob(fileModel);

            //Assert
            Assert.NotNull(result);
            var badResponse = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(400,badResponse.StatusCode);
            _mockUploadDataService.Verify(x => x.UploadFileToBlob(fileData, tenantId), Times.Once());
        }

        [Fact]
        public async void UploadFileToBlob_FailToSendMessageToSericeBus()
        {
            //Arrange
            var tenantId = Guid.NewGuid().ToString();
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile fileData = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            var fileModel = new FileModel()
            {
                FileData = fileData
            };
            _mockUploadDataService.Setup(x => x.UploadFileToBlob(fileData, tenantId))
                                  .ReturnsAsync(true)
                                  .Verifiable();
            _mockServiceBusService.Setup(x => x.SendMessageToServiceBusAsync(fileModel.FileData.FileName, tenantId, It.IsAny<string>()))
                                  .ReturnsAsync(false)
                                  .Verifiable();

            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            var fakeHttpContext = new Mock<HttpContext>();
            var claims = new List<Claim>()
            {
                new Claim("TenantId", tenantId),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            fakeHttpContext.Setup(t => t.User).Returns(claimsPrincipal);
            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Object.HttpContext = fakeHttpContext.Object;
            controller.ControllerContext = controllerContext.Object;

            //Act
            var result = await controller.UploadFileToBlob(fileModel);

            //Assert
            Assert.NotNull(result);
            var badResponse = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, badResponse.StatusCode);
            _mockUploadDataService.Verify(x => x.UploadFileToBlob(fileData, tenantId), Times.Once());
            _mockServiceBusService.Verify(x => x.SendMessageToServiceBusAsync(fileModel.FileData.FileName, tenantId, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void GetStatus_Done()
        {
            //Arrange
            var jobStatusId = Guid.NewGuid();
            var jobStatusModel = new JobStatusModel()
            { 
                MessageID = jobStatusId.ToString(),
                TenantID  = Guid.NewGuid().ToString(),
                TextMessage ="Test",
                Success = true
            };

            _mockJobStatusService.Setup(x => x.GetJobStatus(jobStatusId))
                                .Returns(jobStatusModel)
                                .Verifiable();
            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            //Act
            var result = controller.GetStatus(jobStatusId);

            //Assert
            Assert.NotNull(result);
            var okResponse = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResponse.StatusCode);
            var resultJobStatus = Assert.IsType<JobStatusModel>(okResponse.Value);
            Assert.Equal(jobStatusModel.MessageID, resultJobStatus.MessageID);
            Assert.Equal(jobStatusModel.TenantID, resultJobStatus.TenantID);
            Assert.Equal(jobStatusModel.TextMessage, resultJobStatus.TextMessage);
            Assert.True(resultJobStatus.Success);
            _mockJobStatusService.Verify(x => x.GetJobStatus(jobStatusId), Times.Once());
        }

        [Fact]
        public void GetStatus_InProgress()
        {
            //Arrange
            var jobStatusId = Guid.NewGuid();
            
            _mockJobStatusService.Setup(x => x.GetJobStatus(jobStatusId))
                                .Returns(null as JobStatusModel)
                                .Verifiable();
            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            //Act
            var result = controller.GetStatus(jobStatusId);

            //Assert
            Assert.NotNull(result);
            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, response.StatusCode);
            _mockJobStatusService.Verify(x => x.GetJobStatus(jobStatusId), Times.Once());

        }

        [Fact]
        public void GetStatus_Fail()
        {
            //Arrange
            var messageId = Guid.NewGuid();
           
            _mockJobStatusService.Setup(x => x.GetJobStatus(messageId))
                                .Throws(new Exception())
                                .Verifiable();

            var controller = new UploadToBlobController(_mockUploadDataService.Object, _mockServiceBusService.Object, _mockLogger.Object, _mockJobStatusService.Object);

            //Act
            var result = controller.GetStatus(messageId);

            //Assert
            Assert.NotNull(result);
            var badResponse = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, badResponse.StatusCode);
            _mockJobStatusService.Verify(x => x.GetJobStatus(messageId), Times.Once());

        }

    }
}
