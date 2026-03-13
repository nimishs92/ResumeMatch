using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;

namespace ResumeJobMatcher.Infrastructure.Services;

public class JobDescriptionService : IJobDescriptionService
{
    private readonly ILogger<JobDescriptionService> _logger;

    public JobDescriptionService(ILogger<JobDescriptionService> logger)
    {
        _logger = logger;
    }

    public Task<JobDescription> GetJobDescriptionAsync(string id)
    {
        // Implementation would load from database or file system
        throw new NotImplementedException();
    }

    public Task<IEnumerable<JobDescription>> GetAllJobDescriptionsAsync()
    {
        // Implementation would load all job descriptions
        throw new NotImplementedException();
    }
}
