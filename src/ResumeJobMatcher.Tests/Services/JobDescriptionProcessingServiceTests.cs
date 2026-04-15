using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using ResumeJobMatcher.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace ResumeJobMatcher.Tests.Services
{
    public class JobDescriptionProcessingServiceTests
    {
        private readonly Mock<ILogger<JobDescriptionProcessingService>> _loggerMock;
        private readonly Mock<IJobDescriptionService> _jobDescriptionServiceMock;
        private readonly Mock<ILlmService> _llmServiceMock;
        private readonly JobDescriptionProcessingService _service;

        public JobDescriptionProcessingServiceTests()
        {
            _loggerMock = new Mock<ILogger<JobDescriptionProcessingService>>();
            _jobDescriptionServiceMock = new Mock<IJobDescriptionService>();
            _llmServiceMock = new Mock<ILlmService>();
            _service = new JobDescriptionProcessingService(
                _loggerMock.Object,
                _jobDescriptionServiceMock.Object,
                _llmServiceMock.Object);
        }

        [Fact]
        public async Task ProcessJobDescriptionAsync_WithValidFile_ShouldReturnJobDescription()
        {
            // Arrange
            var filePath = "test.txt";
            var fileName = "test.txt";
            var content = "Test job description content";
            
            _llmServiceMock.Setup(x => x.GenerateResponseAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"title\": \"Test Job\", \"companyName\": \"Test Company\"}");
            
            // Act
            var result = await _service.ProcessJobDescriptionAsync(filePath, fileName);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Id);
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public async Task ProcessJobDescriptionAsync_WithEmptyFile_ShouldHandleGracefully()
        {
            // Arrange
            var filePath = "empty.txt";
            var fileName = "empty.txt";
            var content = "";
            
            _llmServiceMock.Setup(x => x.GenerateResponseAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");
            
            // Act
            var result = await _service.ProcessJobDescriptionAsync(filePath, fileName);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Id);
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public async Task ExtractStructuredInformation_WithValidJson_ShouldParseCorrectly()
        {
            // Arrange
            var content = "Software Engineer position with 5+ years experience";
            
            _llmServiceMock.Setup(x => x.GenerateResponseAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"title\": \"Software Engineer\", \"requiredExperience\": \"5+ years\"}");
            
            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "ExtractStructuredInformation");

            var result = await (Task<JobDescription>)methodInfo.Invoke(_service, new object[] { content, "test.txt" });
            
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Id);
        }
    }
}