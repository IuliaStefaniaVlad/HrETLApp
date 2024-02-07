using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrappServices.Tests
{
    public class UploadDataServiceTests
    {
        private Mock<BlobContainerClient> _mockBlobContainerClient = new Mock<BlobContainerClient>();
        private Mock<BlobClient> _mockBlobClient = new Mock<BlobClient>();
        private Mock<ILogger<UploadDataService>> _mockLogger = new Mock<ILogger<UploadDataService>>();

        [Fact]
        public async void UploadFileToBlob_OK()
        {
            //Arrange
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            string tenantId = Guid.NewGuid().ToString();
            
            Response<BlobContentInfo> response = Mock.Of<Response<BlobContentInfo>>(x => x.GetRawResponse().Status == 201);
           
            _mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                                   .Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(response)
                          .Verifiable();


            var service = new UploadDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var result = await service.UploadFileToBlob(file, tenantId);

            //Assert
            Assert.True(result);
            _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once());
        }

        //Keep it as duplicate code to emphasize the expected behavior.
        //I could have used [Theory] and [InlineData()] in the previous test.
        [Fact]
        public async void UploadFIleToBlob_FailUpload()
        {
            //Arrange
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            string tenantId = Guid.NewGuid().ToString();

            Response<BlobContentInfo> response = Mock.Of<Response<BlobContentInfo>>(x => x.GetRawResponse().Status == 400);

            _mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                                   .Returns(_mockBlobClient.Object);

            _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(response)
                          .Verifiable();

            var service = new UploadDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var result = await service.UploadFileToBlob(file, tenantId);

            //Assert
            Assert.False(result);
            _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async void UploadFileToBlob_NullFile()
        {
            //Arrange
            var service = new UploadDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var result = await service.UploadFileToBlob(null, String.Empty);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async void UploadFileToBlob_EmptyFileName()
        {
            //Arrange
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", null);

           
            var service = new UploadDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var result = await service.UploadFileToBlob(file, "This should be a Guid...");

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async void UploadFileToBlob_EmptyGuid()
        {
            //Arrange
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");

            var service = new UploadDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var result = await service.UploadFileToBlob(file, String.Empty);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async void UploadFileToBlob_ThrowException()
        {
            //Arrange
            var data = $"{1},Test,Test,{DateTime.Now.ToString("yyyy-MM-dd")},{1000}";
            var bytes = Encoding.ASCII.GetBytes(data);
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "FileData", "Test.csv");
            var exception = new Exception("Null container client");

            _mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                                   .Throws(exception)
                                   .Verifiable();

            var service = new UploadDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var result = await service.UploadFileToBlob(file, Guid.NewGuid().ToString());

            //Assert
            Assert.False(result);
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
