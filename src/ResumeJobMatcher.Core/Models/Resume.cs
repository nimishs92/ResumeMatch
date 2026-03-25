namespace ResumeJobMatcher.Core.Models;

public class Resume
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Keywords { get; set; } = new();
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LinkedInProfile { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    
    // Professional Summary
    public string Summary { get; set; } = string.Empty;
    
    // Work Experience
    public List<WorkExperience> WorkExperience { get; set; } = new();
    
    // Education
    public List<Education> Education { get; set; } = new();
    
    // Skills
    public List<Skill> Skills { get; set; } = new();
    
    // Certifications
    public List<Certification> Certifications { get; set; } = new();
    
    // Languages
    public List<Language> Languages { get; set; } = new();
    
    // Professional Affiliations
    public List<string> ProfessionalAffiliations { get; set; } = new();
}

public class WorkExperience
{
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
}

public class Education
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Grade { get; set; } = string.Empty;
}

public class Skill
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // e.g., Beginner, Intermediate, Advanced
    public string Category { get; set; } = string.Empty; // e.g., Technical, Soft, Language
}

public class Certification
{
    public string Name { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    public string CredentialUrl { get; set; } = string.Empty;
}

public class Language
{
    public string Name { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty; // e.g., Native, Professional, Basic
}