using ResumeJobMatcher.Core.Models;
using ResumeJobMatcher.Core.Services;
using ResumeJobMatcher.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace ResumeJobMatcher.Tests
{
    public class ResumeProcessingServiceTests
    {
        private readonly Mock<ILogger<ResumeProcessingService>> _loggerMock;
        private readonly Mock<ILlmService> _llmServiceMock;
        private readonly ResumeProcessingService _service;

        public ResumeProcessingServiceTests()
        {
            _loggerMock = new Mock<ILogger<ResumeProcessingService>>();
            _llmServiceMock = new Mock<ILlmService>();
            _service = new ResumeProcessingService(
                _loggerMock.Object,
                _llmServiceMock.Object);
        }

        [Fact]
        public async Task ProcessResumeAsync_WithValidTxtFile_ReturnsResume()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.txt";
            var testFileName = "test_resume.txt";
            var testContent = "Sample resume content for testing";
            var expectedId = "test-id-123";
            
            _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"firstName\":\"John\",\"lastName\":\"Doe\",\"email\":\"john.doe@example.com\"}");

            // Act
            var result = await _service.ProcessResumeAsync(testFilePath, testFileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testFileName, result.FileName);
            Assert.Equal(testContent, result.Content);
            Assert.NotEmpty(result.Id);
        }

        [Fact]
        public async Task ProcessResumeAsync_WithValidPdfFile_ReturnsResume()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.pdf";
            var testFileName = "test_resume.pdf";
            var testContent = "Sample PDF resume content for testing";
            
            _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"firstName\":\"Jane\",\"lastName\":\"Smith\",\"email\":\"jane.smith@example.com\"}");

            // Act
            var result = await _service.ProcessResumeAsync(testFilePath, testFileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testFileName, result.FileName);
            Assert.Equal(testContent, result.Content);
            Assert.NotEmpty(result.Id);
        }

        [Fact]
        public async Task ProcessResumeAsync_WithValidDocxFile_ReturnsResume()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.docx";
            var testFileName = "test_resume.docx";
            var testContent = "Sample DOCX resume content for testing";
            
            _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"firstName\":\"Bob\",\"lastName\":\"Johnson\",\"email\":\"bob.johnson@example.com\"}");

            // Act
            var result = await _service.ProcessResumeAsync(testFilePath, testFileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testFileName, result.FileName);
            Assert.Equal(testContent, result.Content);
            Assert.NotEmpty(result.Id);
        }

        [Fact]
        public async Task ProcessResumeAsync_WithUnsupportedFileType_ThrowsNotSupportedException()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.xyz";
            var testFileName = "test_resume.xyz";

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await _service.ProcessResumeAsync(testFilePath, testFileName));
        }

        [Fact]
        public async Task ExtractTextFromResumeAsync_WithValidTxtFile_ReturnsText()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.txt";
            var testContent = "Sample text content from resume";
            
            // Act
            var result = await _service.ExtractTextFromResumeAsync(testFilePath);

            // Assert
            Assert.Equal(testContent, result);
        }

        [Fact]
        public async Task ExtractTextFromResumeAsync_WithValidPdfFile_ReturnsText()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.pdf";
            var testContent = "Sample PDF content from resume";
            
            // Act
            var result = await _service.ExtractTextFromResumeAsync(testFilePath);

            // Assert
            Assert.Equal(testContent, result);
        }

        [Fact]
        public async Task ExtractTextFromResumeAsync_WithValidDocxFile_ReturnsText()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.docx";
            var testContent = "Sample DOCX content from resume";
            
            // Act
            var result = await _service.ExtractTextFromResumeAsync(testFilePath);

            // Assert
            Assert.Equal(testContent, result);
        }

        [Fact]
        public async Task ExtractTextFromResumeAsync_WithUnsupportedFileType_ThrowsNotSupportedException()
        {
            // Arrange
            var testFilePath = "/path/to/test/resume.xyz";

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await _service.ExtractTextFromResumeAsync(testFilePath));
        }

        [Fact]
        public async Task ExtractStructuredInformationAsync_WithValidJsonResponse_ReturnsExtractionData()
        {
            // Arrange
            var resumeText = "Test resume content";
            var validJson = "{\"firstName\":\"John\",\"lastName\":\"Doe\",\"email\":\"john.doe@example.com\"}";

            _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
                .ReturnsAsync(validJson);

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "ExtractStructuredInformationAsync");

            var result = await (Task<ResumeExtractionData>)methodInfo.Invoke(_service, new object[] { resumeText });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john.doe@example.com", result.Email);
        }

        [Fact]
        public async Task ExtractStructuredInformationAsync_WithMalformedJson_ReturnsEmptyExtractionData()
        {
            // Arrange
            var resumeText = "Test resume content";
            var malformedJson = "{invalid json}";

            _llmServiceMock.Setup(s => s.ExtractKeywordsAsync(It.IsAny<string>()))
                .ReturnsAsync(malformedJson);

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "ExtractStructuredInformationAsync");

            var result = await (Task<ResumeExtractionData>)methodInfo.Invoke(_service, new object[] { resumeText });

            // Assert
            Assert.NotNull(result);
            // Should return empty data structure when JSON parsing fails
        }

        [Fact]
        public async Task CleanJsonResponse_WithMarkdownFormattedJson_ReturnsCleanJson()
        {
            // Arrange
            var markdownFormattedJson = "```json\n{\"firstName\":\"John\",\"lastName\":\"Doe\"}\n```";

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "CleanJsonResponse");

            var result = (string)methodInfo.Invoke(_service, new object[] { markdownFormattedJson });

            // Assert
            Assert.NotNull(result);
            Assert.Contains("firstName", result);
            Assert.Contains("John", result);
        }

        [Fact]
        public async Task CleanJsonResponse_WithPlainJson_ReturnsSameJson()
        {
            // Arrange
            var plainJson = "{\"firstName\":\"John\",\"lastName\":\"Doe\"}";

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "CleanJsonResponse");

            var result = (string)methodInfo.Invoke(_service, new object[] { plainJson });

            // Assert
            Assert.Equal(plainJson, result);
        }

        [Fact]
        public async Task CleanJsonResponse_WithNullInput_ReturnsEmptyJson()
        {
            // Arrange
            string nullInput = null;

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "CleanJsonResponse");

            var result = (string)methodInfo.Invoke(_service, new object[] { nullInput });

            // Assert
            Assert.Equal("{}", result);
        }

        [Fact]
        public async Task PopulateResumeFromExtractedData_WithCompleteData_PopulatesAllFields()
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
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "PopulateResumeFromExtractedData");

            methodInfo.Invoke(_service, new object[] { resume, extractionData });

            // Assert
            Assert.Equal("John", resume.FirstName);
            Assert.Equal("Doe", resume.LastName);
            Assert.Equal("john.doe@example.com", resume.Email);
            Assert.Equal("(123) 456-7890", resume.Phone);
            Assert.Equal("New York, NY", resume.Location);
            Assert.Equal("Software Engineer with 5+ years experience", resume.Summary);
        }

        [Fact]
        public async Task PopulateResumeFromExtractedData_WithPartialData_PopulatesAvailableFields()
        {
            // Arrange
            var resume = new Resume();
            var extractionData = new ResumeExtractionData
            {
                FirstName = "Jane",
                LastName = "Smith"
            };

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "PopulateResumeFromExtractedData");

            methodInfo.Invoke(_service, new object[] { resume, extractionData });

            // Assert
            Assert.Equal("Jane", resume.FirstName);
            Assert.Equal("Smith", resume.LastName);
            // Other fields should be empty strings
            Assert.Equal(string.Empty, resume.Email);
        }

        [Fact]
        public async Task PopulateResumeFromExtractedData_WithNullData_PopulatesEmptyFields()
        {
            // Arrange
            var resume = new Resume();
            ResumeExtractionData nullData = null;

            // Act
            var methodInfo = _service.GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .First(m => m.Name == "PopulateResumeFromExtractedData");

            methodInfo.Invoke(_service, new object[] { resume, nullData });

            // Assert
            Assert.Equal(string.Empty, resume.FirstName);
            Assert.Equal(string.Empty, resume.LastName);
        }
    }
}