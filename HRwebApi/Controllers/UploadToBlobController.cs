using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HrappServices.Interfaces;
using HrappModels;

namespace HRwebApi.Controllers
{
    [Route("Upload")]
    [ApiController]
    public class UploadToBlobController : ControllerBase
    {
        private readonly IUploadDataService _uploadService;
        private readonly IServiceBusService _serviceBusService;
        private readonly IJobStatusService _jobStatusService;
        private readonly ILogger<UploadToBlobController> _logger;

        public UploadToBlobController(IUploadDataService uploadService, IServiceBusService serviceBus, ILogger<UploadToBlobController> logger, IJobStatusService jobStatusService)
        {
            _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
            _serviceBusService = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobStatusService = jobStatusService;
        }

        [HttpPost]
        [Authorize]
        [Route("UploadFileToBlob")]
        public async Task<IActionResult> UploadFileToBlob([FromForm] FileModel formFile)
        {
            try
            {
                if(Path.GetExtension(formFile.FileData.FileName).ToLower() != ".csv")
                {
                    return BadRequest("Invalid file format.");
                }
                var tenantId = User.Claims.Single(i => i.Type == "TenantId").Value;
                var result = await _uploadService.UploadFileToBlob(formFile.FileData, tenantId);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }
                //put message in service bus queue to trigger azure function
                var jobStatusId = Guid.NewGuid().ToString();
                var success = await _serviceBusService.SendMessageToServiceBusAsync(formFile.FileData.FileName, tenantId, jobStatusId);
                if (!success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                return Ok(jobStatusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        [Authorize]
        [Route("GetStatus")]
        public IActionResult GetStatus ([FromBody] Guid jobStatusId)
        {
            try
            {
                var status = _jobStatusService.GetJobStatus(jobStatusId);
                if (status != null)
                {
                    return Ok(status);
                }
                return StatusCode(StatusCodes.Status404NotFound, "Could not find job. Processing data might not finished.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }
    }
}
