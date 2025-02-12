using Kilde.Application.ApplicationServices;
using Kilde.Application.Models;
using Kilde.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Kilde.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SourceController : ControllerBase
    {
        private readonly ISourceService _sourceApplicationService;

        public SourceController(ISourceService sourceApplicationService)
        {
            _sourceApplicationService = sourceApplicationService;
        }

        /// <summary>
        /// Returns all Sources stored in Kilde
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _sourceApplicationService.GetSources();
            return Ok(result);
        }

        /// <summary>
        /// Creates a new Source. 
        /// The endpoint is idempotent. 
        /// If a source with the given name already exists it will not create it, but return already existing.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateSource([FromBody] Source source)
        {
            try
            {
                var result = await _sourceApplicationService.CreateSource(source);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
