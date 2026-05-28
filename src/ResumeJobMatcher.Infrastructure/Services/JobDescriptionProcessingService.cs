using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ResumeJobMatcher.Infrastructure.Services;

public class JobDescriptionProcessingService : IJobDescriptionProcessingService
{
    private readonly ILogger<JobDescriptionProcessingService> _logger;
    private readonly IJobDescriptionService _jobDescriptionService;
    private readonly ILlmService _llmService;

    public JobDescriptionProcessingService(
        ILogger<JobDescriptionProcessingService> logger,
        IJobDescriptionService jobDescriptionService,
        ILlmService llmService)
    {
        _logger = logger;
        _jobDescriptionService = jobDescriptionService;
        _llmService = llmService;
    }

    public async Task<JobDescription> ProcessJobDescriptionAsync(string filePath, string fileName)
    {
        try
        {
            _logger.LogInformation("Starting job description processing for file: {FileName}", fileName);
            
            // Extract text content from the file
            var content = await ExtractTextFromJobDescriptionFile(filePath, fileName);
            
            // Use LLM to extract structured information
            var jobDescription = await ExtractStructuredInformation(content, fileName);
            
            // Add metadata
            jobDescription.Id = Guid.NewGuid().ToString();
            jobDescription.CreatedAt = DateTime.UtcNow;
            
            // Store the processed job description
            await StoreJobDescription(jobDescription);
            
            _logger.LogInformation("Successfully processed job description: {JobId}", jobDescription.Id);
            return jobDescription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job description: {FileName}", fileName);
            throw;
        }
    }

    private async Task<string> ExtractTextFromJobDescriptionFile(string filePath, string fileName)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return fileExtension switch
        {
            ".txt" => await ExtractTextFromTxtFile(filePath),
            ".pdf" => await ExtractTextFromPdfFile(filePath),
            ".docx" => await ExtractTextFromDocxFile(filePath),
            _ => throw new NotSupportedException($"File type not supported: {fileExtension}")
        };
    }

    private async Task<string> ExtractTextFromTxtFile(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    private async Task<string> ExtractTextFromPdfFile(string filePath)
    {
        // For PDF processing, we would need a PDF library like iTextSharp or PdfPig
        // This is a placeholder implementation - in a real implementation, you'd use a PDF extraction library
        return await File.ReadAllTextAsync(filePath);
    }

    private async Task<string> ExtractTextFromDocxFile(string filePath)
    {
        // For DOCX processing, we would need a library like DocX or ClosedXML
        // This is a placeholder implementation - in a real implementation, you'd use a DOCX extraction library
        return await File.ReadAllTextAsync(filePath);
    }

    private async Task<JobDescription> ExtractStructuredInformation(string content, string fileName)
    {
        var prompt = $@"Extract structured information from the following job description:

{content}

Respond only with valid JSON containing these fields:
- title: string
- companyName: string
- companySize: string
- industry: string
- jobLevel: string
- jobType: string
- remoteOption: string
- location: string
- requiredExperience: string
- requiredEducation: string
- requiredSkills: array of strings
- desiredSkills: array of strings
- preferredExperience: string
- salaryRange: string
- benefits: array of strings
- workHours: string
- applicationInstructions: string
- applicationUrl: string
- postedDate: string (YYYY-MM-DD format, or null if not available)
- expiryDate: string (YYYY-MM-DD format, or null if not available)
- department: string

Return valid JSON only. All newline characters inside JSON string values must be explicitly escaped as \n.";

        try
        {
            var extractionData = await _llmService.GenerateResponseAsyncGenerics<JobDescriptionExtractionData>(prompt);

            if (extractionData != null)
            {
                return MapToJobDescription(content, extractionData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting structured information from LLM, using fallback method.");
        }

        // Fallback to basic information extraction
        _logger.LogWarning("Could not parse structured response from LLM. Using fallback method.");
        return new JobDescription
        {
            Content = content,
            Keywords = ExtractKeywords(content)
        };
    }

    private JobDescription MapToJobDescription(string content, JobDescriptionExtractionData extraction)
    {
        return new JobDescription
        {
            Content = content,
            Keywords = ExtractKeywords(content),
            Title = extraction.Title ?? string.Empty,
            CompanyName = extraction.CompanyName ?? string.Empty,
            CompanySize = extraction.CompanySize ?? string.Empty,
            Industry = extraction.Industry ?? string.Empty,
            JobLevel = extraction.JobLevel ?? string.Empty,
            JobType = extraction.JobType ?? string.Empty,
            RemoteOption = extraction.RemoteOption ?? string.Empty,
            Location = extraction.Location ?? string.Empty,
            RequiredExperience = extraction.RequiredExperience ?? string.Empty,
            RequiredEducation = extraction.RequiredEducation ?? string.Empty,
            RequiredSkills = extraction.RequiredSkills ?? new List<string>(),
            DesiredSkills = extraction.DesiredSkills ?? new List<string>(),
            PreferredExperience = extraction.PreferredExperience ?? string.Empty,
            SalaryRange = extraction.SalaryRange ?? string.Empty,
            Benefits = extraction.Benefits ?? new List<string>(),
            WorkHours = extraction.WorkHours ?? string.Empty,
            ApplicationInstructions = extraction.ApplicationInstructions ?? string.Empty,
            ApplicationUrl = extraction.ApplicationUrl ?? string.Empty,
            PostedDate = extraction.PostedDate ?? DateTime.UtcNow,
            ExpiryDate = extraction.ExpiryDate,
            Department = extraction.Department ?? string.Empty
        };
    }

    private List<string> ExtractKeywords(string content)
    {
        // Simple keyword extraction logic
        var keywords = new List<string>();
        
        // This is a simplified version - in a real implementation, you'd use NLP techniques
        var words = content.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Extract common technical keywords (this is a simplified example)
        foreach (var word in words)
        {
            var cleanWord = word.Trim(",.!?;:\"'()[]{}".ToCharArray()).ToLower();
            if (cleanWord.Length > 3 && !string.IsNullOrEmpty(cleanWord))
            {
                keywords.Add(cleanWord);
            }
        }
        
        return keywords.Distinct().ToList();
    }

    private async Task StoreJobDescription(JobDescription jobDescription)
    {
        // Store the processed job description as a JSON file
        var storagePath = Path.Combine("storage", "job-descriptions");
        Directory.CreateDirectory(storagePath);
        
        var fileName = $"{jobDescription.Id}.json";
        var filePath = Path.Combine(storagePath, fileName);
        
        var json = JsonSerializer.Serialize(jobDescription, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
        
        _logger.LogInformation("Stored job description to: {FilePath}", filePath);
    }
}

public class JobDescriptionExtractionData
{
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanySize { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string JobLevel { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string RemoteOption { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string RequiredExperience { get; set; } = string.Empty;
    public string RequiredEducation { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public List<string> DesiredSkills { get; set; } = new();
    public string PreferredExperience { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public string WorkHours { get; set; } = string.Empty;
    public string ApplicationInstructions { get; set; } = string.Empty;
    public string ApplicationUrl { get; set; } = string.Empty;
    public DateTime? PostedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Department { get; set; } = string.Empty;
}