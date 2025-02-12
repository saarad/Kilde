using Kilde.Application.ApplicationServices;
using Kilde.Application.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Kilde.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        /// <summary>
        /// Gets all articles stored in Kilde
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _articleService.GetArticles();
            return Ok(result);
        }

        /// <summary>
        /// Gets all articles for the specified Source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpGet("{source}")]
        public async Task<IActionResult> Get(string source)
        {
            var result = await _articleService.GetArticlesBySource(source);
            return Ok(result);
        }

        /// <summary>
        /// Creates article on the specified source.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="article">If identifier is not specified, a new guid will be generated</param>
        /// <returns></returns>
        [HttpPost("{source}")]
        public async Task<IActionResult> CreateArticle([FromRoute] string source, [FromBody] ArticleInputModel article)
        {
            try
            {
                var result = await _articleService.CreateArticle(article, source);
                return Ok(result);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all articles for the given Sources
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IEnumerable<string> sources)
        {
            var result = await _articleService.GetArticlesBySources(sources);
            return Ok(result);
        }
    }
}
