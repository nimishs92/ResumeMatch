using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IJobDescriptionService
{
    Task<JobDescription> GetJobDescriptionAsync(string id);
    Task<IEnumerable<JobDescription>> GetAllJobDescriptionsAsync();
}