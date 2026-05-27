using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using ResumeJobMatcher.Infrastructure.Serialization;

namespace ResumeJobMatcher.Infrastructure.Services;

public class ResumeProcessingService : IResumeProcessingService
{
    private readonly ILogger<ResumeProcessingService> _logger;
    private readonly ILlmService _llmService;
    private readonly string _outputDirectory;

    public ResumeProcessingService(
        ILogger<ResumeProcessingService> logger,
        ILlmService llmService,
        string outputDirectory = "resumes")
    {
        _logger = logger;
        _llmService = llmService;
        _outputDirectory = outputDirectory;
        
        // Create the output directory if it doesn't exist
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public async Task<Resume> ProcessResumeAsync(string filePath, string fileName)
    {
        _logger.LogInformation("Processing resume file: {FileName}", fileName);
        
        try
        {
            // Extract text from the resume file
            var resumeText = await ExtractTextFromResumeAsync(filePath);
            _logger.LogInformation("Extracted text from resume: {FileName}", fileName);
            
            // Create a new Resume object
            var resume = new Resume
            {
                Id = Guid.NewGuid().ToString(),
                FileName = fileName,
                Content = resumeText,
                CreatedAt = DateTime.UtcNow
            };
            
            // Use LLM to extract structured information
            var extractedData = await ExtractStructuredInformationAsync(resumeText);
            
            // Populate the Resume object with extracted data
            PopulateResumeFromExtractedData(resume, extractedData);
            
            // Store the populated Resume object as JSON
            await StoreResumeAsJsonAsync(resume);
            
            _logger.LogInformation("Successfully processed resume: {FileName}", fileName);
            return resume;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing resume: {FileName}", fileName);
            throw;
        }
    }
    
    public async Task<string> ExtractTextFromResumeAsync(string filePath)
    {
        _logger.LogInformation("Extracting text from resume file: {FilePath}", filePath);
        
        try
        {
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            
            var text = fileExtension switch
            {
                ".txt" => await ExtractTextFromTxtFile(filePath),
                ".pdf" => await ExtractTextFromPdfFile(filePath),
                ".docx" => await ExtractTextFromDocxFile(filePath),
                _ => throw new NotSupportedException($"File type not supported: {fileExtension}")
            };
            
            _logger.LogInformation("Successfully extracted text from resume file: {FilePath}", filePath);
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from resume file: {FilePath}", filePath);
            throw;
        }
    }
    
    private async Task<ResumeExtractionData> ExtractStructuredInformationAsync(string resumeText)
    {
        try
        {
            // Create a comprehensive prompt for the LLM to extract structured data
            var prompt = $@"
Extract structured information from the following resume text:

{resumeText}

Respond only with valid JSON containing these fields:
- firstName: string (first name)
- lastName: string (last name)
- email: string (email address)
- phone: string (phone number)
- location: string (city, state, country)
- linkedinProfile: string (LinkedIn URL)
- portfolioUrl: string (portfolio URL)
- summary: string (professional summary)
- workExperience: array of objects with company, position, description, startDate, endDate, isCurrent
- education: array of objects with institution, degree, fieldOfStudy, startDate, endDate, isCurrent, grade
- skills: array of objects with name, level, category
- certifications: array of objects with name, issuingOrganization, issueDate, expirationDate, credentialUrl
- languages: array of objects with name, proficiency
- professionalAffiliations: array of strings

All dates must be formatted strictly as YYYY-MM-DD (e.g., 2026-02-01) to ensure JSON deserialization compatibility. Use 1st day of the month if only month and year are available (e.g., 2026-02-01 for February 2026).
Return valid JSON only. All newline characters inside JSON string values must be explicitly escaped as \n. Do not output literal line breaks within strings.
";
            
            // Call the LLM service to extract structured data
            // var llmResult = await _llmService.GenerateResponseAsyncGenerics<ResumeExtractionData>(prompt);
            
            // // Clean and parse the JSON response
            // var cleanJson = CleanJsonResponse(llmResult);
            
            // // Parse the JSON into our data structure
            // var extractionData = JsonSerializer.Deserialize<ResumeExtractionData>(cleanJson, new JsonSerializerOptions
            // {
            //     PropertyNameCaseInsensitive = true
            // });

            var extractionData = await _llmService.GenerateResponseAsyncGenerics<ResumeExtractionData>(prompt);
            
            return extractionData ?? new ResumeExtractionData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting structured information from resume");
            return new ResumeExtractionData();
        }
    }
    
    private string CleanJsonResponse(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return "{}";
            
        // Remove markdown code block formatting if present
        string cleanJson = responseText.Trim();
        
        if (cleanJson.StartsWith("```json"))
        {
            int startIndex = cleanJson.IndexOf('\n') + 1;
            int endIndex = cleanJson.LastIndexOf('\n');
            if (startIndex > 0 && endIndex > startIndex)
            {
                cleanJson = cleanJson.Substring(startIndex, endIndex - startIndex).Trim();
            }
            else
            {
                // Remove markdown markers entirely
                cleanJson = cleanJson.Substring(7, cleanJson.Length - 10).Trim();
            }
        }
        else if (cleanJson.StartsWith("```"))
        {
            int startIndex = cleanJson.IndexOf('\n') + 1;
            int endIndex = cleanJson.LastIndexOf('\n');
            if (startIndex > 0 && endIndex > startIndex)
            {
                cleanJson = cleanJson.Substring(startIndex, endIndex - startIndex).Trim();
            }
            else
            {
                // Remove markdown markers entirely
                cleanJson = cleanJson.Substring(3, cleanJson.Length - 6).Trim();
            }
        }
        
        return cleanJson;
    }
    
    private void PopulateResumeFromExtractedData(Resume resume, ResumeExtractionData extractionData)
    {
        // Populate basic fields
        resume.FirstName = extractionData.FirstName ?? string.Empty;
        resume.LastName = extractionData.LastName ?? string.Empty;
        resume.Email = extractionData.Email ?? string.Empty;
        resume.Phone = extractionData.Phone ?? string.Empty;
        resume.Location = extractionData.Location ?? string.Empty;
        resume.LinkedInProfile = extractionData.LinkedinProfile ?? string.Empty;
        resume.PortfolioUrl = extractionData.PortfolioUrl ?? string.Empty;
        resume.Summary = extractionData.Summary ?? string.Empty;
        
        // Populate work experience
        if (extractionData.WorkExperience != null)
        {
            foreach (var workExp in extractionData.WorkExperience)
            {
                resume.WorkExperience.Add(new WorkExperience
                {
                    Company = workExp.Company ?? string.Empty,
                    Position = workExp.Position ?? string.Empty,
                    Description = workExp.Description ?? string.Empty,
                    StartDate = workExp.StartDate ?? DateTime.UtcNow,
                    EndDate = workExp.EndDate,
                    IsCurrent = workExp.IsCurrent ?? false
                });
            }
        }
        
        // Populate education
        if (extractionData.Education != null)
        {
            foreach (var edu in extractionData.Education)
            {
                resume.Education.Add(new Education
                {
                    Institution = edu.Institution ?? string.Empty,
                    Degree = edu.Degree ?? string.Empty,
                    FieldOfStudy = edu.FieldOfStudy ?? string.Empty,
                    StartDate = edu.StartDate ?? DateTime.UtcNow,
                    EndDate = edu.EndDate,
                    IsCurrent = edu.IsCurrent ?? false,
                    Grade = edu.Grade ?? string.Empty
                });
            }
        }
        
        // Populate skills
        if (extractionData.Skills != null)
        {
            foreach (var skill in extractionData.Skills)
            {
                resume.Skills.Add(new Skill
                {
                    Name = skill.Name ?? string.Empty,
                    Level = skill.Level ?? string.Empty,
                    Category = skill.Category ?? string.Empty
                });
            }
        }
        
        // Populate certifications
        if (extractionData.Certifications != null)
        {
            foreach (var cert in extractionData.Certifications)
            {
                resume.Certifications.Add(new Certification
                {
                    Name = cert.Name ?? string.Empty,
                    IssuingOrganization = cert.IssuingOrganization ?? string.Empty,
                    IssueDate = cert.IssueDate ?? DateTime.UtcNow,
                    ExpirationDate = cert.ExpirationDate,
                    CredentialUrl = cert.CredentialUrl ?? string.Empty
                });
            }
        }
        
        // Populate languages
        if (extractionData.Languages != null)
        {
            foreach (var lang in extractionData.Languages)
            {
                resume.Languages.Add(new Language
                {
                    Name = lang.Name ?? string.Empty,
                    Proficiency = lang.Proficiency ?? string.Empty
                });
            }
        }
        
        // Populate professional affiliations
        if (extractionData.ProfessionalAffiliations != null)
        {
            resume.ProfessionalAffiliations.AddRange(extractionData.ProfessionalAffiliations);
        }
        
        // Extract and set keywords
        if (!string.IsNullOrEmpty(resume.Content))
        {
            var keywords = _llmService.ExtractKeywordsAsync(resume.Content).Result;
            resume.Keywords = keywords.Split(',').Select(k => k.Trim()).ToList();
        }
    }
    
    private async Task StoreResumeAsJsonAsync(Resume resume)
    {
        try
        {
            var fileName = $"{resume.Id}.json";
            var filePath = Path.Combine(_outputDirectory, fileName);
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(resume, options);
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation("Resume stored as JSON: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing resume as JSON");
            throw;
        }
    }
    
    private async Task<string> ExtractTextFromTxtFile(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    private async Task<string> ExtractTextFromPdfFile(string filePath)
    {
        // For PDF processing, we would need a PDF library like iTextSharp or PdfPig
        // This is a placeholder implementation - in a real implementation, you'd use a PDF extraction library
        // For now, just return the file contents as text
        return await File.ReadAllTextAsync(filePath);
    }

    private async Task<string> ExtractTextFromDocxFile(string filePath)
    {
        // For DOCX processing, we would need a library like DocX or ClosedXML
        // This is a placeholder implementation - in a real implementation, you'd use a DOCX extraction library
        // For now, just return the file contents as text
        return await File.ReadAllTextAsync(filePath);
    }
}

public class ResumeExtractionData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LinkedinProfile { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<WorkExperienceExtraction> WorkExperience { get; set; } = new();
    public List<EducationExtraction> Education { get; set; } = new();
    public List<SkillExtraction> Skills { get; set; } = new();
    public List<CertificationExtraction> Certifications { get; set; } = new();
    public List<LanguageExtraction> Languages { get; set; } = new();
    public List<string> ProfessionalAffiliations { get; set; } = new();
}

public class WorkExperienceExtraction
{
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    [JsonConverter(typeof(PresentDateConverter))]
    public DateTime? EndDate { get; set; }
    public bool? IsCurrent { get; set; }
}

public class EducationExtraction
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    [JsonConverter(typeof(PresentDateConverter))]
    public DateTime? EndDate { get; set; }
    public bool? IsCurrent { get; set; }
    public string Grade { get; set; } = string.Empty;
}

public class SkillExtraction
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class CertificationExtraction
{
    public string Name { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string CredentialUrl { get; set; } = string.Empty;
}

public class LanguageExtraction
{
    public string Name { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty;
}