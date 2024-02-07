using HrappModels;
using HrappServices.Interfaces;
using Microsoft.Azure.ServiceBus;
using Moq;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ETLFunction.Tests
{
    public class ETLFunctionTests
    {
        private Mock<IExtractDataService> _mockExtractDataService = new Mock<IExtractDataService>();
        private Mock<ITransformDataService> _mockTransformDataService = new Mock<ITransformDataService>();
        private Mock<IEmployeesService> _mockEmployeesService = new Mock<IEmployeesService>();
        private Mock<IJobStatusService> _mockJobStatusService = new Mock<IJobStatusService>();
        private Mock<ILogger<ETLFunction>> _mockLogger = new Mock<ILogger<ETLFunction>>();
        private Mock<ServiceBusMessageActions> _mockMessageActions = new Mock<ServiceBusMessageActions>();
        private readonly string tenantId = Guid.NewGuid().ToString();

        private readonly List<EmployeeRawDataModel> employeesRawData = new List<EmployeeRawDataModel>
            {
                new EmployeeRawDataModel
                {
                    EmployeeID = 1,
                    FirstName = "Ion",
                    LastName = "Popescu",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    GrossAnnualSalary = 60000
                }
            };

        [Fact]
        public async Task ETLFunction_OK()
        {
            //Arrange
            var fileName = "Test.csv";
            var messageId = Guid.NewGuid().ToString();

            List<EmployeeDBModel> employees = new List<EmployeeDBModel>
            {
                new EmployeeDBModel
                {
                    EmployeeID = 1,
                    TenantID = tenantId,
                    FirstName = "Ion",
                    LastName = "Popescu",
                    BirthDate = new DateTime(1990, 5, 15),
                    AnnualIncome = (decimal)41000
                }
            };
            _mockExtractDataService.Setup(x => x.ExtractDataAsync(It.IsAny<string>()))
                                  .ReturnsAsync(employeesRawData)
                                  .Verifiable();
            _mockTransformDataService.Setup(x => x.TransformData(employeesRawData, tenantId))
                                    .Returns(employees)
                                    .Verifiable();
            _mockEmployeesService.Setup(x => x.AddEmployees(employees))
                                .Verifiable();
            _mockJobStatusService.Setup(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), true))
                                .Verifiable();

            var messageBody = JsonSerializer.Serialize(new ServiceBusQueueMessageModel(fileName, tenantId));
            var binaryData = BinaryData.FromString(messageBody);
            
            var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(binaryData, messageId);
            _mockMessageActions.Setup(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()))
                              .Verifiable();

            var func = new ETLFunction(_mockLogger.Object, _mockExtractDataService.Object, _mockTransformDataService.Object, _mockEmployeesService.Object, _mockJobStatusService.Object);

            //Act
            await func.Run(receivedMessage, _mockMessageActions.Object);

            //Assert
            Assert.Equal(employeesRawData[0].FirstName, employees[0].FirstName);
            Assert.Equal(employeesRawData[0].LastName, employees[0].LastName);
            Assert.Equal(employeesRawData[0].DateOfBirth, employees[0].BirthDate);

            _mockExtractDataService.Verify(x => x.ExtractDataAsync(It.IsAny<string>()), Times.Once());
            _mockTransformDataService.Verify(x => x.TransformData(employeesRawData, tenantId), Times.Once());
            _mockJobStatusService.Verify(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), true), Times.Once());
            _mockMessageActions.Verify(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ETLFunction_ExtractDataFailure()
        {
            //Arrange
            var fileName = "Test.csv";
            var messageId = Guid.NewGuid().ToString();

            List<EmployeeDBModel> employees = new List<EmployeeDBModel>
            {
                new EmployeeDBModel
                {
                    EmployeeID = 1,
                    TenantID = tenantId,
                    FirstName = "Ion",
                    LastName = "Popescu",
                    BirthDate = new DateTime(1990, 5, 15),
                    AnnualIncome = (decimal)41000
                }
            };
            _mockExtractDataService.Setup(x => x.ExtractDataAsync(It.IsAny<string>()))
                                  .ReturnsAsync(null as List<EmployeeRawDataModel>)
                                  .Verifiable();
            _mockTransformDataService.Setup(x => x.TransformData(employeesRawData, tenantId))
                                    .Returns(employees)
                                    .Verifiable();
            _mockEmployeesService.Setup(x => x.AddEmployees(employees))
                                .Verifiable();
            _mockJobStatusService.Setup(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), false))
                                .Verifiable();

            var messageBody = JsonSerializer.Serialize(new ServiceBusQueueMessageModel(fileName, tenantId));
            var binaryData = BinaryData.FromString(messageBody);

            var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(binaryData, messageId);
            _mockMessageActions.Setup(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()))
                              .Verifiable();

            var func = new ETLFunction(_mockLogger.Object, _mockExtractDataService.Object, _mockTransformDataService.Object, _mockEmployeesService.Object, _mockJobStatusService.Object);

            //Act
            await func.Run(receivedMessage, _mockMessageActions.Object);

            //Assert
            
            _mockExtractDataService.Verify(x => x.ExtractDataAsync(It.IsAny<string>()), Times.Once());
            _mockTransformDataService.Verify(x => x.TransformData(employeesRawData, tenantId), Times.Never());
            _mockEmployeesService.Verify(x => x.AddEmployees(employees), Times.Never());
            _mockJobStatusService.Verify(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), false), Times.Once());
            _mockMessageActions.Verify(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ETLFunction_TransformDataFailure()
        {
            //Arrange
            var fileName = "Test.csv";
            var messageId = Guid.NewGuid().ToString();

            List<EmployeeDBModel> employees = new List<EmployeeDBModel>
            {
                new EmployeeDBModel
                {
                    EmployeeID = 1,
                    TenantID = tenantId,
                    FirstName = "Ion",
                    LastName = "Popescu",
                    BirthDate = new DateTime(1990, 5, 15),
                    AnnualIncome = (decimal)41000
                }
            };
            _mockExtractDataService.Setup(x => x.ExtractDataAsync(It.IsAny<string>()))
                                  .ReturnsAsync(employeesRawData)
                                  .Verifiable();
            _mockTransformDataService.Setup(x => x.TransformData(employeesRawData, tenantId))
                                    .Returns(null as List<EmployeeDBModel>)
                                    .Verifiable();
            _mockEmployeesService.Setup(x => x.AddEmployees(employees))
                                .Verifiable();
            _mockJobStatusService.Setup(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), false))
                                .Verifiable();

            var messageBody = JsonSerializer.Serialize(new ServiceBusQueueMessageModel(fileName, tenantId));
            var binaryData = BinaryData.FromString(messageBody);

            var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(binaryData, messageId);
            _mockMessageActions.Setup(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()))
                              .Verifiable();

            var func = new ETLFunction(_mockLogger.Object, _mockExtractDataService.Object, _mockTransformDataService.Object, _mockEmployeesService.Object, _mockJobStatusService.Object);

            //Act
            await func.Run(receivedMessage, _mockMessageActions.Object);

            //Assert
            _mockExtractDataService.Verify(x => x.ExtractDataAsync(It.IsAny<string>()), Times.Once());
            _mockTransformDataService.Verify(x => x.TransformData(employeesRawData, tenantId), Times.Once());
            _mockEmployeesService.Verify(x => x.AddEmployees(employees), Times.Never());
            _mockJobStatusService.Verify(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), false), Times.Once());
            _mockMessageActions.Verify(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ETLFunction_UploadDataFailure()
        {
            //Arrange
            var fileName = "Test.csv";
            var tenantId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();

            List<EmployeeDBModel> employees = new List<EmployeeDBModel>
            {
                new EmployeeDBModel
                {
                    EmployeeID = 1,
                    TenantID = tenantId,
                    FirstName = "Ion",
                    LastName = "Popescu",
                    BirthDate = new DateTime(1990, 5, 15),
                    AnnualIncome = (decimal)41000
                }
            };
            var exception = new Exception("Could not load data to database.");
            _mockExtractDataService.Setup(x => x.ExtractDataAsync(It.IsAny<string>()))
                                  .ReturnsAsync(employeesRawData)
                                  .Verifiable();
            _mockTransformDataService.Setup(x => x.TransformData(employeesRawData, tenantId))
                                    .Returns(employees)
                                    .Verifiable();
            _mockEmployeesService.Setup(x => x.AddEmployees(employees))
                                .Throws(exception)
                                .Verifiable();
            _mockJobStatusService.Setup(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), false))
                                .Verifiable();

            var messageBody = JsonSerializer.Serialize(new ServiceBusQueueMessageModel(fileName, tenantId));
            var binaryData = BinaryData.FromString(messageBody);

            var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(binaryData, messageId);
            _mockMessageActions.Setup(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()))
                              .Verifiable();

            var func = new ETLFunction(_mockLogger.Object, _mockExtractDataService.Object, _mockTransformDataService.Object, _mockEmployeesService.Object, _mockJobStatusService.Object);

            //Act
            await func.Run(receivedMessage, _mockMessageActions.Object);

            //Assert
            _mockExtractDataService.Verify(x => x.ExtractDataAsync(It.IsAny<string>()), Times.Once());
            _mockTransformDataService.Verify(x => x.TransformData(employeesRawData, tenantId), Times.Once());
            _mockEmployeesService.Verify(x => x.AddEmployees(employees), Times.Once());
            _mockJobStatusService.Verify(x => x.SetJobStatus(messageId, tenantId, It.IsAny<string>(), false), Times.Once());
            _mockMessageActions.Verify(x => x.CompleteMessageAsync(receivedMessage, It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}