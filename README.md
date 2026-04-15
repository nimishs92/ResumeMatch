# Resume-Job Matcher with Local LLM

This is a .NET 10 application that matches resumes to job descriptions using a locally running LLM (like Llama).

## Features

- Semantic matching of resumes and job descriptions using local LLM
- RESTful API for integration
- Containerized deployment via Docker/Podman
- Configurable LLM endpoint and model settings
- Batch matching capability

## Architecture

The application follows a clean architecture pattern with:
- **API Layer**: ASP.NET Core Web API controllers
- **Core Layer**: Domain models and business logic interfaces
- **Infrastructure Layer**: Implementation of services using HTTP client for LLM communication

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker/Podman
- Local LLM server (e.g., Ollama)

### Setup
1. Clone the repository
2. Install dependencies: `dotnet restore`
3. Run the application: `dotnet run`

### Running with Docker
```bash
docker-compose up --build
```

## API Endpoints

- `POST /api/match` - Match a resume to a job description
- `POST /api/batch-match` - Batch match multiple resumes

## Configuration

The application can be configured through:
- appsettings.json
- Environment variables
- Docker environment variables

## Deployment

The application is containerized and can be deployed using Docker or Podman. The docker-compose.yml file includes both the application and a local LLM server (Ollama) for easy setup.