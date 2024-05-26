using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace REST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SPA : ControllerBase
    {
        private const string Failed = "none";
        private readonly ILogger<SPA> _logger;

        public SPA(ILogger<SPA> logger)
        {
            _logger = logger;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<string>> ProcessSpa(IFormFile file,[FromQuery]string variables,[FromQuery] string query)
        {
            using var streamReader = new StreamReader(file.OpenReadStream());
            var code = await streamReader.ReadToEndAsync();
            code =Regex.Replace(code, @"\r", "");
            var parser = new Parser.Parser();
            parser.CleanData();
            parser.StartParse(code);
            var results = QueryProcessor.QueryProcessor.ProcessQuery(variables + query, testing: true);
            return Ok(results.Count == 0 ? Failed : string.Join(", ", results));
            
        }
    }
}