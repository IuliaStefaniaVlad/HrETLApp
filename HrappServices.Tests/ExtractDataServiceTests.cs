using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using HrappModels;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Globalization;
using System.Text;

namespace HrappServices.Tests
{
    public class ExtractDataServiceTests
    {
        private Mock<ILogger<ExtractDataService>> _mockLogger = new Mock<ILogger<ExtractDataService>>();
        private Mock<BlobContainerClient> _mockBlobContainerClient = new Mock<BlobContainerClient>();
        private Mock<BlobClient> _mockBlobClient = new Mock<BlobClient>();

        [Fact]
        public async void ExtractDataAsync_OK()
        {
            //Arrange
            var blobName = "test.csv";
            List<EmployeeRawDataModel> expectedData = new List<EmployeeRawDataModel>()
            {
                new EmployeeRawDataModel
                {
                    EmployeeID = 1,
                    FirstName = "Test",
                    LastName = "Test",
                    DateOfBirth = DateTime.Now,
                    GrossAnnualSalary = 10000
                }
            };
            
            var csvData = $"{expectedData[0].EmployeeID},{expectedData[0].FirstName},{expectedData[0].LastName},{expectedData[0].DateOfBirth.ToString("yyyy-MM-dd")},{expectedData[0].GrossAnnualSalary}";
            byte[] buffer = Encoding.ASCII.GetBytes(csvData);
            Stream stream = new MemoryStream(buffer, 0, buffer.Length);
            StreamReader sr = new StreamReader(stream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                
            };

            

            _mockBlobContainerClient.Setup(x => x.GetBlobClient(blobName))
                                    .Returns(_mockBlobClient.Object);
            _mockBlobClient.Setup(x => x.OpenRead(It.IsAny<long>(),
                                                 It.IsAny<int?>(),
                                                 It.IsAny<BlobRequestConditions>(),
                                                 It.IsAny<CancellationToken>()))
                          .Returns(stream)
                          .Verifiable();

            
            var mockCsv = new Mock<CsvReader>(sr, config);
            mockCsv.Setup(x => x.GetRecordsAsync<EmployeeRawDataModel>(It.IsAny<CancellationToken>()))
                   .Returns(expectedData.ToAsyncEnumerable());

            var service = new ExtractDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var data = await service.ExtractDataAsync(blobName);

            //Assert
            Assert.NotNull(data);
            Assert.Single(data);
            Assert.Equal(expectedData.Count, data.Count);
            Assert.Equal(expectedData[0].EmployeeID, data[0].EmployeeID);
            Assert.Equal(expectedData[0].FirstName, data[0].FirstName);
            Assert.Equal(expectedData[0].LastName, data[0].LastName);
            Assert.Equal(expectedData[0].DateOfBirth.ToString("yyyy-MM-dd"), data[0].DateOfBirth.ToString("yyyy-MM-dd"));
            Assert.Equal(expectedData[0].GrossAnnualSalary, data[0].GrossAnnualSalary);

            _mockBlobClient.Verify(s => s.OpenRead(It.IsAny<long>(),
                                                 It.IsAny<int?>(),
                                                 It.IsAny<BlobRequestConditions>(),
                                                 It.IsAny<CancellationToken>()), Times.Once());
        }


        [Fact]
        public async void ExtractDataAsync_Fail()
        {
            //Arrange
            var blobName = "test.csv";
            List<EmployeeRawDataModel> expectedData = new List<EmployeeRawDataModel>()
            {
                new EmployeeRawDataModel
                {
                    EmployeeID = 1,
                    FirstName = "Test",
                    LastName = "Test",
                    DateOfBirth = DateTime.Now,
                    GrossAnnualSalary = -4,
                }
            };

            var csvData = $"{expectedData[0].EmployeeID},{expectedData[0].FirstName},{expectedData[0].LastName},{expectedData[0].DateOfBirth.ToString("yyyy-MM-dd")},{expectedData[0].GrossAnnualSalary}";
            byte[] buffer = Encoding.ASCII.GetBytes(csvData);
            Stream stream = new MemoryStream(buffer, 0, buffer.Length);
            StreamReader sr = new StreamReader(stream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,

            };
            var exception = new Exception("Invalid GrossAnnualSalary");

            _mockBlobContainerClient.Setup(x => x.GetBlobClient(blobName))
                                    .Returns(_mockBlobClient.Object);
            _mockBlobClient.Setup(x => x.OpenRead(It.IsAny<long>(),
                                                 It.IsAny<int?>(),
                                                 It.IsAny<BlobRequestConditions>(),
                                                 It.IsAny<CancellationToken>()))
                          .Returns(stream)
                          .Verifiable();


            var mockCsv = new Mock<CsvReader>(sr, config);
            mockCsv.Setup(x => x.GetRecordsAsync<EmployeeRawDataModel>(It.IsAny<CancellationToken>()))
                   .Returns(expectedData.ToAsyncEnumerable());

            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), exception, It.IsAny<Func<object, Exception?, string>>())).Verifiable();

            var service = new ExtractDataService(_mockBlobContainerClient.Object, _mockLogger.Object);

            //Act
            var data = await service.ExtractDataAsync(blobName);

            //Assert
            Assert.Null(data);
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

