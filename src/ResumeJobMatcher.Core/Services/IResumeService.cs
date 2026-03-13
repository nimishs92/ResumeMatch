using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IResumeService
{
    Task<Resume> GetResumeAsync(string id);
    Task<IEnumerable<Resume>> GetAllResumesAsync();
    Task<string> ExtractTextFromResumeAsync(string filePath);
}

public class Resume
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Keywords { get; set; } = new();
}