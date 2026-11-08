using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/report-reasons")]
    public class ReportReasonController : ControllerBase
    {
        private readonly ReportReasonService _service;

        public ReportReasonController(ReportReasonService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }
    }
}