using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ResumeJobMatcher.Infrastructure.Services;

public class ResumeService : IResumeService
{
    private readonly ILogger<ResumeService> _logger;

    public ResumeService(ILogger<ResumeService> logger)
    {
        _logger = logger;
    }

    public Task<Resume> GetResumeAsync(string id)
    {
        // Implementation would load from database or file system
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Resume>> GetAllResumesAsync()
    {
        // Implementation would load all resumes
        throw new NotImplementedException();
    }
}
