using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kilde.WebAPI.Controllers
{
    [Route("api/kildejobs")]
    [ApiController]
    public class KildeJobsController : ControllerBase
    {
        private const string _jobsBaseAddress = "https://kildecronjobs.azurewebsites.net/api";

        /// <summary>
        /// Triggers all jobs manually
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TriggerAllJobs()
        {
            using var httpClient = new HttpClient();
            _ = httpClient.GetAsync("https://kildecronjobs.azurewebsites.net/api/vg");

            return Ok("All jobs triggered");
        }

        /// <summary>
        /// Pings all jobs (used to keep alive)
        /// </summary>
        /// <returns></returns>
        [HttpGet("ping")]
        public async Task<ActionResult> KeepAliveAll()
        {
            using var httpClient = new HttpClient();
            var vgResponse = await httpClient.GetStringAsync("https://kildecronjobs.azurewebsites.net/api/vg/ping");

            return Ok($"All jobs pinged. VG says: {vgResponse}. E24 says: I am not alive. Tekno says: I am not alive");
        }

        /// <summary>
        /// Triggers VG job
        /// </summary>
        /// <returns></returns>
        [HttpGet("vg")]
        public ActionResult TriggerVgJob()
        {
            using var httpClient = new HttpClient();
            _ = httpClient.GetAsync("https://kildecronjobs.azurewebsites.net/api/vg");

            return Ok("Vg job triggered");
        }
        
        /// <summary>
        /// Pings VG job (used to keep alive)
        /// </summary>
        /// <returns></returns>
        [HttpGet("vg/ping")]
        public async Task<ActionResult> KeepAliveVg()
        {
            using var httpClient = new HttpClient();
            var vgResponse = await httpClient.GetStringAsync("https://kildecronjobs.azurewebsites.net/api/vg/ping");

            return Ok($"Vg job pinged. VG says: {vgResponse}.");
        }
    }
}
