using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IJobDescriptionService
{
    Task<JobDescription> GetJobDescriptionAsync(string id);
    Task<IEnumerable<JobDescription>> GetAllJobDescriptionsAsync();
}

public class JobDescription
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Keywords { get; set; } = new();
}