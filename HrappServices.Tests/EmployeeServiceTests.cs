using Microsoft.Extensions.Logging;
using HrappModels;
using HrappRepositories.Interfaces;
using Moq;
using Castle.Core.Logging;
using System;

namespace HrappServices.Tests
{
    public class EmployeeServiceTests
    {
        private Mock<IEmployeesRepository> _mockEmployeesRepo = new Mock<IEmployeesRepository>();
        private Mock<ILogger<EmployeesService>> _mockLogger = new Mock<ILogger<EmployeesService>>();


        [Fact]
        public void AddEmployees_OK()
        {
            //Arrange
            var employees = new List<EmployeeDBModel>() 
            {
                new EmployeeDBModel()
                {
                    EmployeeID = 1,
                    FirstName = "Test",
                    LastName = "Test",
                    BirthDate = DateTime.Now,
                    AnnualIncome = (decimal)10000
                }
            };
            _mockEmployeesRepo.Setup(x => x.AddRange(It.IsAny<List<EmployeeDBModel>>())).Verifiable();

            var employeeService = new EmployeesService(_mockEmployeesRepo.Object, _mockLogger.Object);

            //Act
            employeeService.AddEmployees(employees);

            //Assert
            _mockEmployeesRepo.Verify(s => s.AddRange(It.IsAny<List<EmployeeDBModel>>()), Times.Once());
            
        }

        [Fact]
        public void AddEmployees_Fail()
        {
            //Arrange
            var employees = new List<EmployeeDBModel>()
            {
                new EmployeeDBModel()
                {
                    EmployeeID = 1,
                    FirstName = "Test",
                    LastName = "Test",
                    BirthDate = DateTime.Now,
                    AnnualIncome = (decimal)10000
                }
            };

            var exception = new Exception("Could not add employees.");

            _mockEmployeesRepo.Setup(x => x.AddRange(It.IsAny<List<EmployeeDBModel>>()))
                             .Throws(exception)
                             .Verifiable();

            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), exception, It.IsAny<Func<object, Exception?, string>>())).Verifiable();

            var employeeService = new EmployeesService(_mockEmployeesRepo.Object, _mockLogger.Object);

            //Assert
            Assert.Throws<Exception>(() => employeeService.AddEmployees(employees));
            
        }

        [Fact]
        public void GetEmployee_OK()
        {
            //Arrange
            int employeeId = 1;
            string tenantId = Guid.NewGuid().ToString();
            var expectedDBEmployee = new EmployeeDBModel() 
            {
                EmployeeID = employeeId,
                TenantID = tenantId,
                FirstName = "Test",
                LastName = "Test",
                BirthDate = DateTime.Now,
                AnnualIncome = (decimal)10000
            };

            _mockEmployeesRepo.Setup(x => x.GetEmployee(employeeId, tenantId))
                             .Returns(expectedDBEmployee)
                             .Verifiable();
            var employeeService = new EmployeesService(_mockEmployeesRepo.Object, _mockLogger.Object);

            //Act
            var expectedEmployee = employeeService.GetEmployee(employeeId, tenantId);

            //Assert
            _mockEmployeesRepo.Verify(s => s.GetEmployee(employeeId, tenantId), Times.Once());
            Assert.NotNull(expectedEmployee);
            Assert.Equal(expectedEmployee.FirstName, expectedDBEmployee.FirstName);
            Assert.Equal(expectedEmployee.LastName, expectedDBEmployee.LastName);
            Assert.Equal(expectedEmployee.BirthDate, expectedDBEmployee.BirthDate);
            Assert.Equal(expectedEmployee.AnnualIncome, expectedDBEmployee.AnnualIncome);


        }

        [Fact]
        public void GetEmployee_Null()
        {
            //Arrange
            int employeeId = 1;
            string tenantId = Guid.NewGuid().ToString();
            _mockEmployeesRepo.Setup(x => x.GetEmployee(employeeId, tenantId))
                             .Returns(null as EmployeeDBModel)
                             .Verifiable();
            var employeeService = new EmployeesService(_mockEmployeesRepo.Object, _mockLogger.Object);

            //Act
            var expectedEmployee = employeeService.GetEmployee(employeeId, tenantId);

            //Assert
            _mockEmployeesRepo.Verify(s => s.GetEmployee(employeeId, tenantId), Times.Once());
            Assert.Null(expectedEmployee);
        }

        [Fact]
        public void GetEmployee_Exception_Null()
        {
            //Arrange
            int employeeId = 1;
            string tenantId = Guid.NewGuid().ToString();
            var exception = new Exception("Could not get employee from database.");

            _mockEmployeesRepo.Setup(x => x.GetEmployee(employeeId, tenantId))
                             .Throws(exception)
                             .Verifiable();
            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), exception, It.IsAny<Func<object, Exception?, string>>())).Verifiable();

            var employeeService = new EmployeesService(_mockEmployeesRepo.Object, _mockLogger.Object);

            //Act
           var emp = employeeService.GetEmployee(employeeId, tenantId);

            //Assert
            Assert.Null(emp);
            _mockEmployeesRepo.Verify(s => s.GetEmployee(employeeId, tenantId), Times.Once());
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