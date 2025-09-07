# DotNetBrightener.Identity Demo WebAPI

## Overview

This is a minimal ASP.NET Core WebAPI demo project designed to test and validate the functionality of the DotNetBrightener.Identity module. The demo serves as both a test harness and an integration validation tool with HTTP endpoints for testing Identity functionality.

## Project Structure

```
DotNetBrightener.Identity.Demo/
├── Controllers/
│   └── IdentityController.cs             # API controller for Identity testing
├── DotNetBrightener.Identity.Demo.csproj # Project file with minimal dependencies
├── Program.cs                            # WebAPI startup and configuration
└── README.md                             # This documentation
```

## Dependencies

- **Project Reference**: `../DotNetBrightener.Identity/DotNetBrightener.Identity.csproj`
- **Target Framework**: `net9.0`
- **SDK**: `Microsoft.NET.Sdk.Web` (ASP.NET Core WebAPI)

## Running the Demo

### Prerequisites
- .NET 9.0 SDK
- DotNetBrightener.Identity module must compile successfully

### Start the WebAPI
```bash
dotnet run --project src/Identity/DotNetBrightener.Identity.Demo/
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger` (Development environment)

## API Endpoints

### Root Endpoints
- `GET /` - Health check endpoint
- `GET /identity/info` - Identity module information

### Identity Controller Endpoints
- `GET /api/identity/status` - Service status
- `POST /api/identity/test/create-user` - Test user creation
- `POST /api/identity/test/password` - Test password functionality

## Features

### 1. Swagger Integration
- Automatic API documentation generation
- Interactive testing interface in development mode

### 2. Identity Service Integration
- Automatic registration of Identity services
- Dependency injection of UserManager and PasswordManager
- Graceful handling when services are unavailable

### 3. Comprehensive Testing
- User creation and validation
- Password hashing and verification
- Service availability checking
- Assembly type discovery

### 4. Error Handling
- Structured error responses
- Detailed logging for debugging
- Graceful degradation when services fail