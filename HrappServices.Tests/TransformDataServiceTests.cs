using HrappModels;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrappServices.Tests
{
    public class TransformDataServiceTests
    {
        private Mock<ILogger<TransformDataService>> _mockLogger = new Mock<ILogger<TransformDataService>>();


        [Theory]
        [InlineData(0,0)]
        [InlineData(3000,3000)]
        [InlineData(5000,5000)]
        [InlineData(7000, 6600)]
        [InlineData(20000, 17000)]
        [InlineData(25000, 20000)]

        public void TransformData_OK(double input, double expected) 
        {
            //Arrange
            var tenantId = Guid.NewGuid().ToString();

            IEnumerable<EmployeeRawDataModel> rawData = new List<EmployeeRawDataModel>()
            {
                new EmployeeRawDataModel()
                {
                    EmployeeID = 1,
                    FirstName = "Test",
                    LastName = "Test",
                    DateOfBirth = DateTime.Now,
                    GrossAnnualSalary = input
                }
            };

            List<EmployeeDBModel> expectedData = new List<EmployeeDBModel>()
            {
                new EmployeeDBModel()
                {
                    EmployeeID = 1,
                    FirstName = "Test",
                    LastName = "Test",
                    BirthDate = DateTime.Now,
                    TenantID = tenantId,
                    AnnualIncome = (decimal)expected
                }
            };


            var service = new TransformDataService(_mockLogger.Object);

            //Act
            List<EmployeeDBModel> result = service.TransformData(rawData, tenantId);

            //Arrange
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<EmployeeDBModel>(result[0]);
            Assert.Equal(expectedData[0].EmployeeID, result[0].EmployeeID);
            Assert.Equal(expectedData[0].FirstName, result[0].FirstName);
            Assert.Equal(expectedData[0].LastName, result[0].LastName);
            Assert.Equal(expectedData[0].BirthDate.ToShortDateString(), result[0].BirthDate.ToShortDateString());
            Assert.Equal(expectedData[0].TenantID, result[0].TenantID);
            Assert.Equal(expectedData[0].AnnualIncome, result[0].AnnualIncome);
        }

        [Fact]
        public void TransformData_Fail()
        {
            //Arrange
            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception?, string>>()))
                      .Verifiable();

            var service = new TransformDataService(_mockLogger.Object);

            //Act
            var result = service.TransformData(null, Guid.NewGuid().ToString());

            //Arrange
            Assert.Null(result);
            _mockLogger.Verify(logger => logger.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.Is<EventId>(eventId => eventId.Id == 0),
                            It.Is<It.IsAnyType>((@object, @type) =>  @type.Name == "FormattedLogValues"),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                                                Times.Once());
        }
    }
    
}
