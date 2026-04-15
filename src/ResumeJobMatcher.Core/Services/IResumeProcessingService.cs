using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IResumeProcessingService
{
    Task<Resume> ProcessResumeAsync(string filePath, string fileName);
    Task<string> ExtractTextFromResumeAsync(string filePath);
}