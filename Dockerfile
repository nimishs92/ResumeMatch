# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the solution file and all project files
COPY *.sln .
COPY src/ResumeJobMatcher.Api/*.csproj src/ResumeJobMatcher.Api/
COPY src/ResumeJobMatcher.Core/*.csproj src/ResumeJobMatcher.Core/
COPY src/ResumeJobMatcher.Infrastructure/*.csproj src/ResumeJobMatcher.Infrastructure/
COPY *.props ./

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out /p:UseAppHost=false

# Use the official ASP.NET runtime as the base image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

ENV ASPNETCORE_ENVIRONMENT=Development
# Expose the port the app runs on
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "ResumeJobMatcher.Api.dll"]


