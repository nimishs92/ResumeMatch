using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

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
        // Create a prompt for the LLM to extract structured information
        var prompt = $@"
Extract structured information from the following job description:
{content}

Return the information in JSON format with the following structure:
{{
  ""title"": ""Job title"",
  ""companyName"": ""Company name"",
  ""companySize"": ""Company size"",
  ""industry"": ""Industry"",
  ""jobLevel"": ""Job level"",
  ""jobType"": ""Job type"",
  ""remoteOption"": ""Remote option"",
  ""location"": ""Location"",
  ""requiredExperience"": ""Required experience"",
  ""requiredEducation"": ""Required education"",
  ""requiredSkills"": [""skill1"", ""skill2""],
  ""desiredSkills"": [""skill1"", ""skill2""],
  ""preferredExperience"": ""Preferred experience"",
  ""salaryRange"": ""Salary range"",
  ""benefits"": [""benefit1"", ""benefit2""],
  ""workHours"": ""Work hours"",
  ""applicationInstructions"": ""Application instructions"",
  ""applicationUrl"": ""Application URL"",
  ""postedDate"": ""Posted date"",
  ""expiryDate"": ""Expiry date"",
  ""department"": ""Department""
}}

Extract all relevant information, even if it's not explicitly mentioned. If information is missing, leave the field empty.
";

        // Call LLM service to extract information
        var llmResponse = await _llmService.GenerateResponseAsync(prompt);
        
        // Parse the response and populate the job description object
        try
        {
            // Try to extract JSON from the LLM response
            var jsonResponse = ExtractJsonFromResponse(llmResponse);
            
            // If we got valid JSON, parse it
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var jobDescription = JsonSerializer.Deserialize<JobDescription>(jsonResponse, options);
                
                if (jobDescription != null)
                {
                    // Set content and keywords
                    jobDescription.Content = content;
                    jobDescription.Keywords = ExtractKeywords(content);
                    return jobDescription;
                }
            }
            
            // Fallback to basic information extraction
            _logger.LogWarning("Could not parse structured response from LLM. Using fallback method.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error parsing structured response from LLM, using basic extraction. Error: {Error}", ex.Message);
        }
        
        // Fallback to basic information extraction
        return new JobDescription
        {
            Content = content,
            Keywords = ExtractKeywords(content)
        };
    }

    private string ExtractJsonFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return string.Empty;

        try
        {
            // Check for markdown code blocks and extract JSON
            if (response.StartsWith("```json") && response.EndsWith("```"))
            {
                // Extract content between ```json and ```
                int startIndex = response.IndexOf('\n') + 1;
                int endIndex = response.LastIndexOf('\n');
                if (startIndex > 0 && endIndex > startIndex)
                {
                    return response.Substring(startIndex, endIndex - startIndex).Trim();
                }
                else
                {
                    // Remove the markdown markers entirely
                    return response.Substring(7, response.Length - 10).Trim();
                }
            }
            else if (response.StartsWith("```") && response.EndsWith("```"))
            {
                // Handle generic code blocks
                int startIndex = response.IndexOf('\n') + 1;
                int endIndex = response.LastIndexOf('\n');
                if (startIndex > 0 && endIndex > startIndex)
                {
                    return response.Substring(startIndex, endIndex - startIndex).Trim();
                }
                else
                {
                    // Remove the markdown markers entirely
                    return response.Substring(3, response.Length - 6).Trim();
                }
            }
            else
            {
                // Try to find JSON content in the response (might be unformatted)
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    return response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
            }
            
            return response.Trim();
        }
        catch
        {
            return response.Trim();
        }
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