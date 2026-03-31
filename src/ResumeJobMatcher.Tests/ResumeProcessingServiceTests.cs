using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using ResumeJobMatcher.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace ResumeJobMatcher.Tests;

public class ResumeProcessingServiceTests
{
    private readonly Mock<ILogger<ResumeProcessingService>> _loggerMock;
    private readonly Mock<IResumeService> _resumeServiceMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly ResumeProcessingService _service;

    public ResumeProcessingServiceTests()
    {
        _loggerMock = new Mock<ILogger<ResumeProcessingService>>();
        _resumeServiceMock = new Mock<IResumeService>();
        _llmServiceMock = new Mock<ILlmService>();
        _service = new ResumeProcessingService(
            _loggerMock.Object,
            _resumeServiceMock.Object,
            _llmServiceMock.Object);
    }

    [Fact]
    public async Task ProcessResumeAsync_ExtractsTextAndPopulatesResume()
    {
        // Arrange
        var testFilePath = "/path/to/test/resume.txt";
        var testFileName = "test_resume.txt";
        var testContent = "Sample resume content for testing";
        
        _resumeServiceMock.Setup(s => s.ExtractTextFromResumeAsync(testFilePath))
            .ReturnsAsync(testContent);
            
        _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
            .ReturnsAsync("C#, .NET, Angular, JavaScript");

        // Act
        var result = await _service.ProcessResumeAsync(testFilePath, testFileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testFileName, result.FileName);
        Assert.Equal(testContent, result.Content);
        Assert.NotEmpty(result.Id);
    }

    [Fact]
    public async Task ProcessResumeAsync_CallsExtractTextFromResume()
    {
        // Arrange
        var testFilePath = "/path/to/test/resume.txt";
        var testFileName = "test_resume.txt";
        var testContent = "Sample resume content for testing";
        
        _resumeServiceMock.Setup(s => s.ExtractTextFromResumeAsync(testFilePath))
            .ReturnsAsync(testContent);
            
        _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
            .ReturnsAsync("C#, .NET, Angular, JavaScript");

        // Act
        var result = await _service.ProcessResumeAsync(testFilePath, testFileName);

        // Assert
        _resumeServiceMock.Verify(s => s.ExtractTextFromResumeAsync(testFilePath), Times.Once);
    }

    [Fact]
    public async Task CleanJsonResponse_RemovesMarkdownFormatting()
    {
        // Arrange
        var responseWithMarkdown = "```json\n{\"firstName\":\"John\",\"lastName\":\"Doe\"}\n```";
        
        // Act
        var result = typeof(ResumeProcessingService)
            .GetMethod("CleanJsonResponse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_service, new object[] { responseWithMarkdown }) as string;

        // Assert
        Assert.NotNull(result);
        Assert.Contains("firstName", result);
        Assert.Contains("John", result);
    }

    [Fact]
    public async Task PopulateResumeFromExtractedData_PopulatesAllFields()
    {
        // Arrange
        var resume = new Resume();
        var extractionData = new ResumeExtractionData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "(123) 456-7890",
            Location = "New York, NY",
            Summary = "Software Engineer with 5+ years experience"
        };

        // Act
        typeof(ResumeProcessingService)
            .GetMethod("PopulateResumeFromExtractedData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_service, new object[] { resume, extractionData });

        // Assert
        Assert.Equal("John", resume.FirstName);
        Assert.Equal("Doe", resume.LastName);
        Assert.Equal("john.doe@example.com", resume.Email);
        Assert.Equal("(123) 456-7890", resume.Phone);
        Assert.Equal("New York, NY", resume.Location);
        Assert.Equal("Software Engineer with 5+ years experience", resume.Summary);
    }
}