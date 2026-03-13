namespace ResumeJobMatcher.Core.Models;

public class MatchingSettings
{
    public string LlmEndpoint { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "llama3";
    public double Threshold { get; set; } = 0.7;
    public int MaxResults { get; set; } = 10;
}