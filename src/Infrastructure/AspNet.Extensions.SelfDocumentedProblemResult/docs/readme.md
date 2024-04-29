# ASP.NET Core WebApp Self-Documented Problem Result Extension


&copy; 2024 [DotNet Brightener](mailto:admin@dotnetbrightener.com)


![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult)

## Introduction

Have you ever wanted to have a consistent way of returning errors from your ASP.NET Core Web application? This package provides an abstraction for responding errors from your application to the client, base on the [RFC 9457](https://tools.ietf.org/html/rfc9457) specification. 

When the application encounter an error, it should return a `ProblemDetails` object that contains information about the error. 

This package

- Added a global exception handler to catch unhandled exceptions and return a `ProblemDetails` object. An `ILogger` is also added to log the exception automatically for the unhandled exceptions.

- Provides a based `IProblembResult` interface and its extension methods `ToProblemDetails()` or `ToProblemResult()`, to create consistent error responses. The error response format is based on the [RFC 9457](https://tools.ietf.org/html/rfc9457) specification. 

When your application needs to response the error, you can either throw an exception derived from `BaseProblemDetailsError` class or simply create a class that implements `IProblemResult` interface. The error object will be converted to `ProblemDetails` object and returned to the client. Check [Usage](#usage) section for more information.

## Installation

You can install the package from [NuGet](https://www.nuget.org/packages/DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult):

```bash

dotnet add package DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult

```

## Usage

### 1. Enable the global exception handler
Add the following code to your `Startup.cs` (if you use Startup.cs) or `Program.cs` (by default) file:

```csharp

// this can be omitted if your application already added IHttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// The default way of handling unhandled exceptions
builder.Services.AddExceptionHandler<UnhandledExceptionResponseHandler>();

// Adds services required for creation of <see cref="ProblemDetails"/> for failed requests.
builder.Services.AddProblemDetails();


// Add the following to your Configure method, 
// or after 
// var app = builder.Build(); if you use Program.cs
app.UseExceptionHandler();

```

After the configuration above, if your application throws an exception, the response will be a `ProblemDetails` object.

### 2. Create a consistent error response

#### 2.1. Using Exception approach

Traditionally we used to throw exceptions when there are errors. You can create an exception class that inherits `BaseProblemDetailsError` class. The `<summary>` and `<remarks>` XML comments will be used to generate the `ProblemDetails` object. 

```csharp

/// <summary>
///     The error represents the requested object of type `User` could not be found
/// </summary>
/// <remarks>
///     The error is thrown because the requested resource of type `User` could not be found
/// </remarks>
public class UserNotFoundException : BaseProblemDetailsError
{
    public UserNotFoundException()
        : base("User Not Found", HttpStatusCode.BadRequest)
    {
    }

    public UserNotFoundException(long userId)
        : this()
    {
        Data.Add("userId", userId);
    }
}

```

Somewhere in your application, where an error is expected, you can throw the exception as followed:

```csharp
// UserService.cs

    public User GetUser(long userId)
    {
        var user = _userRepository.GetUser(userId);

        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        return user;
    }

```

Without having to handle the exception, the error will be caught by the global exception handler and return a `ProblemDetails` object.

```csharp
// UserController.cs

    [HttpGet("{userId}")]
    public IActionResult GetUserDetail(long userId)
    {
        var user = _userService.GetUser(userId);

        // Without handling the exception, the error will be caught by the global exception handler

        return Ok(user);
    }

```


#### 2.2. Using ProblemResult approach



Create a class that inherits `BaseProblemDetailsError`. The `<summary>` and `<remarks>` XML comments will be used to generate the `ProblemDetails` object. 

```csharp

using AspNet.Extensions.SelfDocumentedProblemResult.ErrorResults;

/// <summary>
///     The error represents the requested object of type `User` could not be found
/// </summary>
/// <remarks>
///     The error is thrown because the requested resource of type `User` could not be found
/// </remarks>
public class UserNotFoundError : BaseProblemDetailsError
{
    public UserNotFoundError()
        : base(HttpStatusCode.NotFound)
    {

    }

    public UserNotFoundError(long userId)
        : this()
    {
        Data.Add("userId", userId);
    }
}

```

```csharp
// UserService.cs

    public User GetUser(long userId)
    {
        var user = _userRepository.GetUser(userId);

        return user;
    }

```

Somewhere in your controller, where an error is expected, you can return the error like this:



```csharp
// UserController.cs
    [HttpGet("{userId}")]
    public IActionResult GetUserDetail(long userId)
    {
        var user = _userService.GetUser(userId);
        if (user == null)
        {
            // Explicitly return the error
            var error = new UserNotFoundError(userId);

            return error.ToProblemResult();
        }

        // Omited for brevity
    }

```

> In the above snippet, where the user is not found, a response of status code 404 will be returned with the following body:
>
>```json
>
>{
>    "type": "UserNotFoundError",
>    "title": "User Not Found Error",
>    "status": 404,
>    "detail": "The error is thrown because the requested resource of type `User` could not be found",
>    "instance": "/users/123",
>    "data": {
>        "userId": 123
>    }
>}
>
>```

The XML comments for the class will be used to generate the detail information about the error. It can be useful if you use the [UI package](https://www.nuget.org/packages/DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult.UI), as the error information can be obtain via the UI. 