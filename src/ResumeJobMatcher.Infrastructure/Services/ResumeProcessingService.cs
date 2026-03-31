using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ResumeJobMatcher.Infrastructure.Services;

public class ResumeProcessingService : IResumeProcessingService
{
    private readonly ILogger<ResumeProcessingService> _logger;
    private readonly IResumeService _resumeService;
    private readonly ILlmService _llmService;
    private readonly string _outputDirectory;

    public ResumeProcessingService(
        ILogger<ResumeProcessingService> logger,
        IResumeService resumeService,
        ILlmService llmService,
        string outputDirectory = "resumes")
    {
        _logger = logger;
        _resumeService = resumeService;
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
            var resumeText = await _resumeService.ExtractTextFromResumeAsync(filePath);
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

Example structure:
{{
  ""firstName"": ""John"",
  ""lastName"": ""Doe"",
  ""email"": ""john.doe@example.com"",
  ""phone"": ""(123) 456-7890"",
  ""location"": ""New York, NY"",
  ""linkedinProfile"": ""https://linkedin.com/in/johndoe"",
  ""portfolioUrl"": ""https://johndoe.dev"",
  ""summary"": ""Experienced software developer with 5+ years in web applications..."",
  ""workExperience"": [
    {{
      ""company"": ""Tech Corp"",
      ""position"": ""Senior Developer"",
      ""description"": ""Led development of enterprise applications..."",
      ""startDate"": ""2020-01-01"",
      ""endDate"": ""2023-12-31"",
      ""isCurrent"": false
    }}
  ],
  ""education"": [
    {{
      ""institution"": ""University of Technology"",
      ""degree"": ""B.S. Computer Science"",
      ""fieldOfStudy"": ""Computer Science"",
      ""startDate"": ""2016-09-01"",
      ""endDate"": ""2020-06-30"",
      ""isCurrent"": false,
      ""grade"": ""3.8 GPA""
    }}
  ],
  ""skills"": [
    {{
      ""name"": ""C#"",
      ""level"": ""Advanced"",
      ""category"": ""Technical""
    }}
  ],
  ""certifications"": [
    {{
      ""name"": ""AWS Certified Developer"",
      ""issuingOrganization"": ""Amazon Web Services"",
      ""issueDate"": ""2022-01-15"",
      ""expirationDate"": ""2025-01-15"",
      ""credentialUrl"": ""https://aws.amazon.com/certification""
    }}
  ],
  ""languages"": [
    {{
      ""name"": ""English"",
      ""proficiency"": ""Native""
    }}
  ],
  ""professionalAffiliations"": [
    ""IEEE Member"",
    ""ACM Member""
  ]
}}

Respond only with valid JSON. Do not include any additional text or formatting.";
            
            // Call the LLM service to extract structured data
            var llmResult = await _llmService.ExtractKeywordsAsync(prompt);
            
            // Clean and parse the JSON response
            var cleanJson = CleanJsonResponse(llmResult);
            
            // Parse the JSON into our data structure
            var extractionData = JsonSerializer.Deserialize<ResumeExtractionData>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
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
    public DateTime? EndDate { get; set; }
    public bool? IsCurrent { get; set; }
}

public class EducationExtraction
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
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