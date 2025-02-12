using KildeCronJobs.Common.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Kilde.JobsApi.Controllers
{
    [Route("api/vg")]
    [ApiController]
    public class VgController : ControllerBase
    {
        private readonly VGService _vgService;

        public VgController(VGService vgService)
        {
            _vgService = vgService;
        }

        // GET: api/<VgController>
        [HttpGet]
        public ActionResult Get(CancellationToken cancellationToken = default)
        {
            _vgService.DoWork().GetAwaiter().GetResult();
            return Ok("VG Job has been started");
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            return Ok("pong!");
        }
    }
}
