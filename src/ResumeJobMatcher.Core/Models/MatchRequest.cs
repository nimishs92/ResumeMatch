namespace ResumeJobMatcher.Core.Models;

public class MatchRequest
{
    public string JobDescription { get; set; } = string.Empty;
    public string ResumeContent { get; set; } = string.Empty;
    public double Threshold { get; set; } = 0.7;
}

public class BatchMatchRequest
{
    public List<MatchRequest> Requests { get; set; } = new();
}

public class MatchResult
{
    public string JobId { get; set; } = string.Empty;
    public string ResumeId { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public double MatchPercentage { get; set; }
    public List<string> KeyMatches { get; set; } = new();
    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
}