public class SemanticMatchResult
{
    public double Score { get; set; }
    public List<string> Matches { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
