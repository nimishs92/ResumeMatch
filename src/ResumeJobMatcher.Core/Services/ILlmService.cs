using ResumeJobMatcher.Core.Models;
using System.Text.Json;

namespace ResumeJobMatcher.Core.Services;

public interface ILlmService
{
    Task<SemanticMatchResult> CalculateSemanticSimilarityAsync(
        string jobDescription, 
        string resumeContent);
    
    Task<string> ExtractKeywordsAsync(string text);
    Task<string> GenerateMatchSummaryAsync(string jobDescription, string resumeContent);
    Task<string> GenerateResponseAsync(string prompt);
    
    Task<T> GenerateResponseAsyncGenerics<T>(string prompt) where T : class;
}