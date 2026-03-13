---
description: This Document shows the project structure.
---

## Architecture

The application follows a clean architecture pattern with:
- **API Layer**: ASP.NET Core Web API controllers
- **Core Layer**: Domain models and business logic interfaces
- **Infrastructure Layer**: Implementation of services using HTTP client for LLM communication

Here is the directory structure 


├── Directory.Packages.props
├── docker-compose.yml
├── Dockerfile
├── models
│   └── blobs
├── README.md
├── ResumeMatch.sln
└── src
    ├── ResumeJobMatcher.Api
    │   ├── appsettings.json
    │   ├── Controllers
    │   │   └── MatchController.cs
    │   ├── Program.cs
    │   └── ResumeJobMatcher.Api.csproj
    ├── ResumeJobMatcher.Core
    │   ├── Models
    │   │   ├── MatchingSettings.cs
    │   │   ├── MatchRequest.cs
    │   │   └── SemanticMatchResult.cs
    │   ├── ResumeJobMatcher.Core.csproj
    │   └── Services
    │       ├── IJobDescriptionService.cs
    │       ├── ILlmService.cs
    │       ├── IMatcherService.cs
    │       └── IResumeService.cs
    └── ResumeJobMatcher.Infrastructure
        ├── ResumeJobMatcher.Infrastructure.csproj
        └── Services
            ├── JobDescriptionService.cs
            ├── LlmService.cs
            ├── MatcherService.cs
            └── ResumeService.cs


Use tool call to read any file