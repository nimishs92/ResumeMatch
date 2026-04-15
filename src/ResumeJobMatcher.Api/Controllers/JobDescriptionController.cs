using Microsoft.AspNetCore.Mvc;
using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;

namespace ResumeJobMatcher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobDescriptionController : ControllerBase
    {
        private readonly IJobDescriptionProcessingService _jobDescriptionProcessingService;
        private readonly ILogger<JobDescriptionController> _logger;

        public JobDescriptionController(
            IJobDescriptionProcessingService jobDescriptionProcessingService,
            ILogger<JobDescriptionController> logger)
        {
            _jobDescriptionProcessingService = jobDescriptionProcessingService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessJobDescription(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // Ensure the file is a supported type (PDF, DOCX, TXT, etc.)
                var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Unsupported file type. Supported types: PDF, DOCX, TXT");
                }

                // Save the uploaded file temporarily
                var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + fileExtension);
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Process the job description
                var jobDescription = await _jobDescriptionProcessingService.ProcessJobDescriptionAsync(tempFilePath, file.FileName);
                
                // Clean up the temporary file
                System.IO.File.Delete(tempFilePath);

                return Ok(jobDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job description: {FileName}", file.FileName);
                return BadRequest($"Error processing job description: {ex.Message}");
            }
        }

        [HttpGet("processed/{id}")]
        public async Task<IActionResult> GetProcessedJobDescription(string id)
        {
            try
            {
                // This would normally load from storage, but for now returning a placeholder
                // In a real implementation, you'd have a storage service for persisted job descriptions
                return Ok(new { message = "Job description retrieved", id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving processed job description: {Id}", id);
                return BadRequest($"Error retrieving job description: {ex.Message}");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}