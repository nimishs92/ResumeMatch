using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Llama.Grammar.Service;
using LLama.Native;
using System.Text.Json;

namespace ResumeJobMatcher.Infrastructure.Services;

public class LlamaSharpLlmService : ILlmService
{
    private readonly MatchingSettings _settings;
    private readonly ILogger<LlamaSharpLlmService> _logger;
    private readonly Kernel _kernel;

    public LlamaSharpLlmService(IOptions<MatchingSettings> settings, ILogger<LlamaSharpLlmService> logger, Kernel kernel)
    {
        _settings = settings.Value;
        _logger = logger;
        _kernel = kernel;
    }

    public async Task<SemanticMatchResult> CalculateSemanticSimilarityAsync(
        string jobDescription, 
        string resumeContent)
    {
        try
        {
            _logger.LogDebug("Calculating semantic similarity between job description and resume");
            
            // In a real implementation, this would use LlamaSharp to call a local LLM
            // Example of what this would look like:
            /*
            // Initialize LlamaSharp model
            var model = new LlamaSharp.LlamaModel("path/to/model.gguf");
            
            // Generate response using LlamaSharp
            var prompt = $@"
            Analyze the semantic similarity between a job description and a resume.
            Job Description: {jobDescription}
            Resume Content: {resumeContent}

            Provide your analysis in JSON format with these fields:
            - score: A decimal between 0.0 and 1.0 representing similarity
            - matches: An array of strings describing key matching points
            - summary: A brief summary of the match

            Respond only with valid JSON.";

            var response = await model.GenerateTextAsync(prompt);
            */
            
            // For now, returning placeholder result
            return new SemanticMatchResult
            {
                Score = 0.85,
                Matches = new List<string> 
                { 
                    "Relevant experience in software development",
                    "Experience with .NET technologies",
                    "Strong problem-solving skills" 
                },
                Summary = "Good match based on experience and skills alignment"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling LLM service for semantic similarity");
            throw;
        }
    }

    public async Task<string> ExtractKeywordsAsync(string text)
    {
        try
        {
            _logger.LogDebug("Extracting keywords from text");
            
            // In a real implementation, this would use LlamaSharp to extract keywords
            // Example:
            /*
            var prompt = $@"
            Extract key keywords from the following text:
            {text}

            Respond only with a comma-separated list of keywords.";
            
            var model = new LlamaSharp.LlamaModel("path/to/model.gguf");
            var response = await model.GenerateTextAsync(prompt);
            return response;
            */
            
            // Placeholder result
            return "software,development,c#,dotnet,programming";
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
            _logger.LogDebug("Generating match summary");
            
            // In a real implementation, this would use LlamaSharp to generate a summary
            // Example:
            /*
            var prompt = $@"
            Generate a summary of how well the resume matches the job description.
            Job Description: {jobDescription}
            Resume Content: {resumeContent}

            Provide a concise summary of the match.";
            
            var model = new LlamaSharp.LlamaModel("path/to/model.gguf");
            var response = await model.GenerateTextAsync(prompt);
            return response;
            */
            
            // Placeholder result
            return "The candidate has strong experience in software development with relevant skills for this role.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating match summary");
            return string.Empty;
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Generating LLM response for prompt: {Prompt}", prompt);
            
            // In a real implementation, this would use LlamaSharp to generate text
            // Example:
            /*
            var model = new LlamaSharp.LlamaModel("path/to/model.gguf");
            var response = await model.GenerateTextAsync(prompt);
            return response;
            */
            
            // Placeholder result
            return $"Generated response for: {prompt}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LLM response");
            throw;
        }
    }

    public async Task<T> GenerateResponseAsyncGenerics<T>(string prompt) where T : class
    {
        try
        {
            _logger.LogInformation("Generating LLM response for generic type: {TypeName}", typeof(T).Name);
            
            // This is a simplified placeholder implementation
            // In a real implementation, this would:
            // 1. Call the LLM with the prompt
            // 2. Use GBNF grammar to constrain output to valid JSON
            // 3. Deserialize the JSON response to type T
            
            // For demonstration purposes, we return default(T)  
            // In a real implementation, we'd use the kernel to make the call and process the result
            // Example:
            /*
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);
            
            var response = await _kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageAsync(chatHistory);
            
            var result = JsonSerializer.Deserialize<T>(response.Content);
            return result;
            */
            _logger.LogInformation("Generating LLM model path");
            string modelPath = Path.Combine("models", "gemma-4-E2B-it-Q4_K_M.gguf"); // change it to your own model path.
            _logger.LogInformation("LLM model path {0}", modelPath);

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 16384, // The longest length of chat as memory.
                GpuLayerCount = 0  //0 ensures CPU-only execution

            };
            using var model = LLamaWeights.LoadFromFile(parameters);
            using var context = model.CreateContext(parameters);
            var executor = new StatelessExecutor(model, parameters);

            // 2. Convert the class to a GBNF grammar
            IGbnfGrammar grammarConverter = new GbnfGrammar();
            string gbnf = grammarConverter.ConvertTypeToGbnf<T>();

            // 3. Set up the inference with the restricted grammar
            var inferenceParams = new InferenceParams
            {
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Grammar = new Grammar(gbnf, "root"),
                    Temperature = 0.7f, // Temperature is also here now
                    TopK = 40
                }
            };

            string result = "";
            await foreach (var text in executor.InferAsync(prompt, inferenceParams))
            {
                result += text;
            }

            _logger.LogInformation("LLM response generated: {Result}", result);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<T>(result, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating LLM response for generic type {TypeName}", typeof(T).Name);
            throw;
        }
    }
}