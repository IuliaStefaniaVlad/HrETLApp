using Azure;
using HrappModels;
using HrappServices.Interfaces;
using HRwebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace HrappControllers.Tests
{
    public class EmployeesControllerTests
    {
        private Mock<IEmployeesService> _mockEmployeesService = new Mock<IEmployeesService>();
        private Mock<ILogger<EmployeesController>> _mockLogger = new Mock<ILogger<EmployeesController>>();
        private Mock<HttpContext> _fakeHttpContext = new Mock<HttpContext>();
        private Mock<ControllerContext> _controllerContext = new Mock<ControllerContext>();


        [Fact]
        public void GetEmployee_OK()
        {
            //Arrange
            var employeeId = 1;
            var tenantId = Guid.NewGuid().ToString();
            var employee = new EmployeeModel()
            {
                FirstName = "Test",
                LastName = "Test",
                BirthDate = new DateTime(1990, 5, 15),
                AnnualIncome = (decimal)60000
            };
            _mockEmployeesService.Setup(x => x.GetEmployee(employeeId, tenantId))
                                .Returns(employee)
                                .Verifiable();

            var controller = new EmployeesController(_mockEmployeesService.Object, _mockLogger.Object);

            var claims = new List<Claim>()
            {
                new Claim("TenantId", tenantId),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _fakeHttpContext.Setup(t => t.User).Returns(claimsPrincipal);
            _controllerContext.Object.HttpContext = _fakeHttpContext.Object;
            controller.ControllerContext = _controllerContext.Object;

            //Act
            var result = controller.GetEmployee(employeeId);

            //Assert
            Assert.NotNull(result);
            var okResponse = Assert.IsType<OkObjectResult>(result);
            var resultModel = Assert.IsType<EmployeeModel>(okResponse.Value);
            Assert.NotNull(resultModel);
            Assert.Equal(employee.FirstName, resultModel.FirstName);
            Assert.Equal(employee.LastName, resultModel.LastName);
            Assert.Equal(employee.BirthDate, resultModel.BirthDate);
            Assert.Equal(employee.AnnualIncome, resultModel.AnnualIncome);
            _mockEmployeesService.Verify(x => x.GetEmployee(employeeId, tenantId), Times.Once());
        }

        [Fact]
        public void GetEmployee_TenantNotFound()
        {
            //Arrange
            var employeeId = 1;

            _mockLogger.Setup(x => x.Log(LogLevel.Error, 0, It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception?, string>>())).Verifiable();

            var controller = new EmployeesController(_mockEmployeesService.Object, _mockLogger.Object);

            //Act
            var result = controller.GetEmployee(employeeId);

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
        public void GetEmployee_EmployeeNotFound()
        {
            //Arrange
            var employeeId = 1;
            var tenantId = Guid.NewGuid().ToString();
            
            _mockEmployeesService.Setup(x => x.GetEmployee(employeeId, tenantId))
                                .Returns(null as EmployeeModel)
                                .Verifiable();

            var controller = new EmployeesController(_mockEmployeesService.Object, _mockLogger.Object);

            var claims = new List<Claim>()
            {
                new Claim("TenantId", tenantId),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _fakeHttpContext.Setup(t => t.User).Returns(claimsPrincipal);
            _controllerContext.Object.HttpContext = _fakeHttpContext.Object;
            controller.ControllerContext = _controllerContext.Object;

            //Act
            var result = controller.GetEmployee(employeeId);

            //Assert
            Assert.NotNull(result);
            var badResponse = Assert.IsType<ObjectResult>(result);
            Assert.Equal(badResponse.StatusCode, 404);
            _mockEmployeesService.Verify(x => x.GetEmployee(employeeId, tenantId), Times.Once());
        }
    }
}
