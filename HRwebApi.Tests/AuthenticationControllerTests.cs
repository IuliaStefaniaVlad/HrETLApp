using Castle.Core.Logging;
using HrappModels;
using HRwebApi.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace HrappControllers.Tests
{
    public class AuthenticationControllerTests
    {
        private Mock<UserManager<IdentityUser>> _mgr = MockManager.MockUserManager<IdentityUser>();
        private Mock<ILogger<AuthenticationController>> _mockLogger = new Mock<ILogger<AuthenticationController>>();
        private Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        [Fact]
        public async void Register_OK()
        {
            //arrange
            _mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<IdentityUser>());
            _mgr.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            var configuration = new Mock<IConfiguration>();

            var controller = new AuthenticationController(_mgr.Object, configuration.Object, _mockLogger.Object);
            var model = new TenantInfoModel() { Name = "TestTenant", Password = "TestTenant1!" };
            //act
            var response = await controller.Register(model);

            //assert
            Assert.NotNull(response);
            var okResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal("Succeeded", okResponse?.Value?.ToString());
        }

        [Fact]
        public async void Register_IncorrectPassword()
        {
            //arrange

            var identityErrors = new IdentityError[]
            {
                new IdentityError()
                {
                    Code = "PasswordTooShort",
                    Description = "Passwords must be at least 6 characters."
                },
                new IdentityError()
                {
                    Code = "PasswordRequiresNonAlphanumeric",
                    Description =  "Passwords must have at least one non alphanumeric character."
                }
            };
            _mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<IdentityUser>());
            _mgr.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Failed(identityErrors)));

            var controller = new AuthenticationController(_mgr.Object, _configuration.Object, _mockLogger.Object);
            var model = new TenantInfoModel() { Name = "TestTenant", Password = "teS5!ser" };
            //act
            var response = await controller.Register(model);

            //assert
            Assert.NotNull(response);
            var brResponse = Assert.IsType<ObjectResult>(response);
            var errorArray = Assert.IsType<IdentityResult>(brResponse.Value);
            Assert.Contains("Failed", brResponse?.Value?.ToString());
            Assert.Equal(2, errorArray.Errors.Count());
        }

        [Fact]
        public async void Register_TenantAlreadyExists()
        {
            //arrange
            var identityErrors = new IdentityError[]
                {
                    new IdentityError()
                    {
                        Code = "TenantExists",
                        Description = "Tenant already registered."
                    }
                };
            var model = new TenantInfoModel() { Name = "TestTenant", Password = "TestTenant1!" };
            _mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser() { UserName = model.Name });
            var controller = new AuthenticationController(_mgr.Object, _configuration.Object, _mockLogger.Object);

            //act
            var response = await controller.Register(model);

            //assert
            Assert.NotNull(response);
            var brResponse = Assert.IsType<ObjectResult>(response);
            var errorArray = Assert.IsType<IdentityError[]>(brResponse.Value);
            Assert.Single(errorArray);
            Assert.Contains(identityErrors[0].Code, errorArray[0].Code);
        }

        [Fact]
        public async void Login_OK()
        {
            //arrange
            var model = new TenantInfoModel() { Name = "TestTenant", Password = "TestTenant1!" };
            _mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser() { UserName = model.Name });
            _mgr.Setup(x => x.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _mgr.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string>());


            _configuration.SetupGet(x => x[It.Is<string>(s => s == "JWTSecret")]).Returns("ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM");
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "JWTValidIssuer")]).Returns("http://localhost:61955");
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "JWTValidAudience")]).Returns("http://localhost:5001");


            var controller = new AuthenticationController(_mgr.Object, _configuration.Object, _mockLogger.Object);

            //act
            var response = await controller.Login(model);

            //assert
            Assert.NotNull(response);
            var okResponse = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(okResponse.Value);
            Assert.Contains("token", okResponse.Value.ToString());
            Assert.Contains("expiration", okResponse.Value.ToString());

        }

        [Fact]
        public async void Login_IncorrectPassword()
        {
            //arrange
            var model = new TenantInfoModel() { Name = "TestTenant", Password = "TestTenant1!" };
            _mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser() { UserName = model.Name });
            _mgr.Setup(x => x.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(false);
            _mgr.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string>());


            _configuration.SetupGet(x => x[It.Is<string>(s => s == "JWTSecret")]).Returns("ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM");
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "JWTValidIssuer")]).Returns("http://localhost:61955");
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "JWTValidAudience")]).Returns("http://localhost:5001");


            var controller = new AuthenticationController(_mgr.Object, _configuration.Object, _mockLogger.Object);

            //act
            var response = await controller.Login(model);

            //assert
            Assert.NotNull(response);
            var okResponse = Assert.IsType<UnauthorizedResult>(response);
            Assert.Equal(401, okResponse.StatusCode);

        }

        [Fact]
        public async void Login_InvalidUsername()
        {
            //arrange
            var model = new TenantInfoModel() { Name = "TestTenant", Password = "TestTenant1!" };
            _mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(null as IdentityUser);

            var controller = new AuthenticationController(_mgr.Object, _configuration.Object, _mockLogger.Object);

            //act
            var response = await controller.Login(model);

            //assert
            Assert.NotNull(response);
            var okResponse = Assert.IsType<UnauthorizedResult>(response);
            Assert.Equal(401, okResponse.StatusCode);

        }
    }
}