using Microsoft.AspNetCore.Mvc;
using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;

namespace ResumeJobMatcher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IResumeProcessingService _resumeProcessingService;
        private readonly IJobDescriptionProcessingService _jobDescriptionProcessingService;
        private readonly IMatcherService _matcherService;

        public MatchController(
            IResumeProcessingService resumeProcessingService,
            IJobDescriptionProcessingService jobDescriptionProcessingService,
            IMatcherService matcherService)
        {
            _resumeProcessingService = resumeProcessingService;
            _jobDescriptionProcessingService = jobDescriptionProcessingService;
            _matcherService = matcherService;
        }

        [HttpPost("match")]
        public async Task<IActionResult> MatchResumeToJob([FromBody] MatchRequest request)
        {
            if (request == null)
                return BadRequest("Match request cannot be null");

            try
            {
                // Removed calls to services that are not implemented
                // We'll directly use the content passed in the request for matching
                var result = await _matcherService.MatchAsync("1" , "1", request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during matching: {ex.Message}");
            }
        }

        [HttpGet("results/{id}")]
        public async Task<IActionResult> GetMatchResult(string id)
        {
            try
            {
                // You'll need to implement this method in your service or add a repository
                // For now, returning a placeholder
                return Ok(new { message = "Match result retrieved", id = id });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving match result: {ex.Message}");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}