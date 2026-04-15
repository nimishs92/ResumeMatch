using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IResumeService
{
    Task<Resume> GetResumeAsync(string id);
    Task<IEnumerable<Resume>> GetAllResumesAsync();
}