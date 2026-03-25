namespace ResumeJobMatcher.Core.Models;

public class JobDescription
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Keywords { get; set; } = new();
    
    // Company Information
    public string CompanyName { get; set; } = string.Empty;
    public string CompanySize { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    
    // Job Details
    public string JobLevel { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string RemoteOption { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    // Requirements
    public string RequiredExperience { get; set; } = string.Empty;
    public string RequiredEducation { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    
    // Desired Qualifications
    public List<string> DesiredSkills { get; set; } = new();
    public string PreferredExperience { get; set; } = string.Empty;
    
    // Compensation & Benefits
    public string SalaryRange { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public string WorkHours { get; set; } = string.Empty;
    
    // Application Process
    public string ApplicationInstructions { get; set; } = string.Empty;
    public string ApplicationUrl { get; set; } = string.Empty;
    
    // Posting Metadata
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public string Department { get; set; } = string.Empty;
}