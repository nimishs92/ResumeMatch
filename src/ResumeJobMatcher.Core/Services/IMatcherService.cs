using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface IMatcherService
{
    Task<MatchResult> MatchAsync(string resumeId, string jobId, MatchRequest request);
    Task<IEnumerable<MatchResult>> BatchMatchAsync(BatchMatchRequest request);
}


