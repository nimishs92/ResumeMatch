using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IJobDescriptionProcessingService
{
    Task<JobDescription> ProcessJobDescriptionAsync(string filePath, string fileName);
}