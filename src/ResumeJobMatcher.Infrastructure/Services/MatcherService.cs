using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;

namespace ResumeJobMatcher.Infrastructure.Services;

public class MatcherService : IMatcherService
{
    private readonly ILlmService _llmService;
    private readonly ILogger<MatcherService> _logger;

    public MatcherService(ILlmService llmService, ILogger<MatcherService> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<MatchResult> MatchAsync(string resumeId, string jobId, MatchRequest request)
    {
        try
        {
            _logger.LogInformation("Starting match process for resume {ResumeId} and job {JobId}", resumeId, jobId);
            
            // Use LLM to calculate semantic similarity
            var semanticResult = await _llmService.CalculateSemanticSimilarityAsync(
                request.JobDescription, 
                request.ResumeContent);

            var result = new MatchResult
            {
                JobId = jobId,
                ResumeId = resumeId,
                SimilarityScore = semanticResult.Score,
                MatchPercentage = semanticResult.Score * 100,
                KeyMatches = semanticResult.Matches,
                MatchedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Match completed with score {Score} for resume {ResumeId}", 
                result.SimilarityScore, resumeId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during matching process for resume {ResumeId}", resumeId);
            throw;
        }
    }

    public async Task<IEnumerable<MatchResult>> BatchMatchAsync(BatchMatchRequest request)
    {
        var results = new List<MatchResult>();
        foreach (var matchRequest in request.Requests)
        {
            var result = await MatchAsync("batch", "batch", matchRequest);
            results.Add(result);
        }
        return results;
    }

    // Removed the conflicting MatchAsync method that returned SemanticMatchResult
    // The service now only has the two methods that properly handle MatchRequest objects
}
