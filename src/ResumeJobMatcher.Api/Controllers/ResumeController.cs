using Microsoft.AspNetCore.Mvc;
using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;

namespace ResumeJobMatcher.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeProcessingService _resumeProcessingService;
        private readonly ILogger<ResumeController> _logger;

        public ResumeController(
            IResumeProcessingService resumeProcessingService,
            ILogger<ResumeController> logger)
        {
            _resumeProcessingService = resumeProcessingService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessResume([FromForm] IFormFile file)
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

                // Process the resume
                var resume = await _resumeProcessingService.ProcessResumeAsync(tempFilePath, file.FileName);
                
                // Clean up the temporary file
                System.IO.File.Delete(tempFilePath);

                return Ok(resume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing resume: {FileName}", file.FileName);
                return BadRequest($"Error processing resume: {ex.Message}");
            }
        }

        [HttpGet("processed/{id}")]
        public async Task<IActionResult> GetProcessedResume(string id)
        {
            try
            {
                // This would normally load from storage, but for now returning a placeholder
                // In a real implementation, you'd have a storage service for persisted resumes
                return Ok(new { message = "Resume retrieved", id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving processed resume: {Id}", id);
                return BadRequest($"Error retrieving resume: {ex.Message}");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}