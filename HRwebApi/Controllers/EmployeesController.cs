using HrappServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRwebApi.Controllers
{
    [Route("Employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeesService _employeesService;
        private readonly ILogger<EmployeesController> _logger;


        public EmployeesController(IEmployeesService employeesService, ILogger<EmployeesController> logger)
        {
            _employeesService = employeesService ?? throw new ArgumentNullException(nameof(employeesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize]
        [Route("GetEmployee")]
        public IActionResult GetEmployee([FromBody] int employeeID)
        {
            try
            {
                var tenantId = User.Claims.Single(i => i.Type == "TenantId").Value;

                var employee = _employeesService.GetEmployee(employeeID, tenantId);
                if (employee == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Could not find employee.");
                }
                return Ok(employee);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
