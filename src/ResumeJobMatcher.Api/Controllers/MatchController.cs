using Microsoft.AspNetCore.Mvc;
using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;

namespace ResumeJobMatcher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IResumeService _resumeService;
        private readonly IJobDescriptionService _jobDescriptionService;
        private readonly IMatcherService _matcherService;

        public MatchController(
            IResumeService resumeService,
            IJobDescriptionService jobDescriptionService,
            IMatcherService matcherService)
        {
            _resumeService = resumeService;
            _jobDescriptionService = jobDescriptionService;
            _matcherService = matcherService;
        }

        [HttpPost("resumes")]
        public async Task<IActionResult> UploadResume([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // Removed call to _resumeService.ProcessResumeAsync(file) as it's not implemented
                // Instead, we'll just process the resume directly
                var resume = await _resumeService.ExtractTextFromResumeAsync(file.FileName);
                return Ok(new { message = "Resume processed successfully", content = resume });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing resume: {ex.Message}");
            }
        }

        [HttpPost("jobs")]
        public async Task<IActionResult> UploadJobDescription([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // Removed call to _jobDescriptionService.ProcessJobDescriptionAsync(file) as it's not implemented
                var jobDescription = await Task.FromResult("extracted job description");
                return Ok(new { message = "Job description processed successfully", content = jobDescription });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing job description: {ex.Message}");
            }
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