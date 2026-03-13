using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ResumeJobMatcher.Infrastructure.Services;
public class LlmService : ILlmService
{
    private readonly Kernel _kernel;
    private readonly MatchingSettings _settings;
    private readonly ILogger<LlmService> _logger;

    public LlmService(Kernel kernel, IOptions<MatchingSettings> settings, ILogger<LlmService> logger)
    {
        _kernel = kernel;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<SemanticMatchResult> CalculateSemanticSimilarityAsync(
        string jobDescription, 
        string resumeContent)
    {
        try
        {
            // Prepare the prompt for the LLM
            var prompt = $@"
Analyze the semantic similarity between a job description and a resume.
Job Description: {jobDescription}
Resume Content: {resumeContent}

Provide your analysis in JSON format with these fields:
- score: A decimal between 0.0 and 1.0 representing similarity
- matches: An array of strings describing key matching points
- summary: A brief summary of the match

Respond only with valid JSON.";

            // Create the function to call the LLM
            var function = _kernel.CreateFunctionFromPrompt(prompt, new PromptExecutionSettings
            {
                ModelId = _settings.ModelName
            });

            // Execute the function
            var result = await _kernel.InvokeAsync(function);
            
            var responseText = result.ToString();
            
            // Log the extracted response text
            _logger.LogDebug("LLM response: {ResponseText}", responseText);
            
            if (!string.IsNullOrEmpty(responseText))
            {
                try
                {
                    // Remove markdown code block formatting if present
                    string cleanJson = responseText.Trim();
                    
                    // Check if response is wrapped in markdown code blocks and extract JSON
                    if (cleanJson.StartsWith("```json") && cleanJson.EndsWith("```"))
                    {
                        // Extract content between ```json and ```
                        int startIndex = cleanJson.IndexOf('\n') + 1;
                        int endIndex = cleanJson.LastIndexOf('\n');
                        if (startIndex > 0 && endIndex > startIndex)
                        {
                            cleanJson = cleanJson.Substring(startIndex, endIndex - startIndex).Trim();
                        }
                        else
                        {
                            // If we can't extract properly, remove the markdown markers entirely
                            cleanJson = cleanJson.Substring(7, cleanJson.Length - 10).Trim();
                        }
                    }
                    else if (cleanJson.StartsWith("```") && cleanJson.EndsWith("```"))
                    {
                        // Handle generic code blocks
                        int startIndex = cleanJson.IndexOf('\n') + 1;
                        int endIndex = cleanJson.LastIndexOf('\n');
                        if (startIndex > 0 && endIndex > startIndex)
                        {
                            cleanJson = cleanJson.Substring(startIndex, endIndex - startIndex).Trim();
                        }
                        else
                        {
                            // Remove the markdown markers entirely
                            cleanJson = cleanJson.Substring(3, cleanJson.Length - 6).Trim();
                        }
                    }
                    
                    // Log the cleaned JSON
                    _logger.LogDebug("Cleaned JSON for parsing: {CleanJson}", cleanJson);
                    
                    // Parse the JSON response
                    using (JsonDocument resultDoc = JsonDocument.Parse(cleanJson))
                    {
                        var scoreElement = resultDoc.RootElement.GetProperty("score");
                        var matchesElement = resultDoc.RootElement.GetProperty("matches");
                        var summaryElement = resultDoc.RootElement.GetProperty("summary");
                        
                        var matches = new List<string>();
                        foreach (var match in matchesElement.EnumerateArray())
                        {
                            matches.Add(match.GetString() ?? string.Empty);
                        }
                        
                        return new SemanticMatchResult
                        {
                            Score = scoreElement.GetDouble(),
                            Matches = matches,
                            Summary = summaryElement.GetString() ?? string.Empty
                        };
                    }
                }
                catch (JsonException ex)
                {
                    // Return default result when JSON parsing fails
                    _logger.LogError(ex, "Failed to parse LLM response JSON: {ResponseContent}", responseText);
                    return new SemanticMatchResult
                    {
                        Score = 0.0,
                        Matches = new List<string>(),
                        Summary = "Error parsing LLM response"
                    };
                }
            }
            
            _logger.LogError("LLM API call failed: Empty response");
            return new SemanticMatchResult { Score = 0.0, Matches = new List<string>(), Summary = "Error in LLM processing" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling LLM service");
            throw;
        }
    }

    public async Task<string> ExtractKeywordsAsync(string text)
    {
        try
        {
            var prompt = $@"
Extract key keywords from the following text:
{text}

Respond only with a comma-separated list of keywords.";
            
            var function = _kernel.CreateFunctionFromPrompt(prompt, new PromptExecutionSettings
            {
                ModelId = _settings.ModelName
            });

            var result = await _kernel.InvokeAsync(function);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting keywords");
            return string.Empty;
        }
    }

    public async Task<string> GenerateMatchSummaryAsync(string jobDescription, string resumeContent)
    {
        try
        {
            var prompt = $@"
Generate a summary of how well the resume matches the job description.
Job Description: {jobDescription}
Resume Content: {resumeContent}

Provide a concise summary of the match.";
            
            var function = _kernel.CreateFunctionFromPrompt(prompt, new PromptExecutionSettings
            {
                ModelId = _settings.ModelName
            });

            var result = await _kernel.InvokeAsync(function);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating match summary");
            return string.Empty;
        }
    }
}