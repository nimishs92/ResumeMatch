using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResumeJobMatcher.Core.Services;
using ResumeJobMatcher.Infrastructure.Services;
using ResumeJobMatcher.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Resume Job Matcher API", 
        Version = "v1" 
    });
    // Register the schema filter for IFormFile handling
    // c.SchemaFilter<FormDataSchemaFilter>();
});

#pragma warning disable SKEXP0070
// Register Semantic Kernel with Llama
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    
    // Configure for local Llama model (Ollama or similar)
    kernelBuilder.AddOllamaChatCompletion(
        modelId: builder.Configuration["MatchingSettings:ModelName"],
        endpoint: new Uri(builder.Configuration["MatchingSettings:LlmEndpoint"]),
        serviceId: "ollama"
    );
    
    return kernelBuilder.Build();
});

// Register the LLM service
builder.Services.AddScoped<ILlmService, LlmService>();

// Register application services
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IResumeProcessingService, ResumeProcessingService>();
builder.Services.AddScoped<IJobDescriptionService, JobDescriptionService>();
builder.Services.AddScoped<IMatcherService, MatcherService>();

// Add configuration for MatchingSettings
builder.Services.Configure<MatchingSettings>(builder.Configuration.GetSection("MatchingSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
