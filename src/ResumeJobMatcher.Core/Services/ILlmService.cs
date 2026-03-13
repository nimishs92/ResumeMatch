using ResumeJobMatcher.Core.Models;

namespace ResumeJobMatcher.Core.Services;

public interface ILlmService
{
    Task<SemanticMatchResult> CalculateSemanticSimilarityAsync(
        string jobDescription, 
        string resumeContent);
    
    Task<string> ExtractKeywordsAsync(string text);
    Task<string> GenerateMatchSummaryAsync(string jobDescription, string resumeContent);
}