# UI for ASP.NET Core Problem Result Extension

&copy; 2024 [DotNet Brightener](mailto:admin@dotnetbrightener.com)

## Introduction

This is an extensions for ASP.Net Core applications that use the [ProblemResult](https://www.nuget.org/packages/DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult) package. This package provides a UI for viewing information about a specific error / exception that can be thrown by the application.

Check out the [ProblemResult](https://www.nuget.org/packages/DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult) package for more details on how to implement self-documented error objects.

## Installation

You can install the package from [NuGet](https://www.nuget.org/packages/DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult.UI):

```bash

dotnet add package DotNetBrightener.AspNet.Extensions.SelfDocumentedProblemResult.UI

```

## Enable UI for viewing errors information

##### 1. Enable generate XML documentation file in your projects / solutions.

Add the following code to the section `<PropertyGroup>` in your your `.csproj` file:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

If you have multiple projects in your solution, you'll need to add the above code to all of the projects. Another way is to add that to the `Directory.Build.props` file in the root of your solution.

##### 2. Configure the ASP.Net project to use the UI package.

Add the following code to your `Startup.cs` (if you use Startup.cs) or `Program.cs` (by default) file:

```csharp

app.MapErrorDocsTrackerUI(options =>
{
    options.UiPath = "/errors-info-ui"; // this is where you can access the UI from your application
    options.ApplicationName = "Test Error"; // this is the name of your application
});

```

## How this work?

When your application is configured to use UI package, the application will collect the error classes that are derived from `IProblemResult`. The error object when returned from the application, will now have `type` properties that leads to the information page of the error.

```json
{
  "type": "https://localhost:7290/errors-info-ui#/WebAppCommonShared.Demo.Controllers.UserNotFoundError",
  "title": "User Not Found Error",
  "status": 404,
  "detail": "The error is thrown because the requested resource of type User could not be found",
  "instance": "/api/ExceptionTest/exception2/123",
  "data": {
    "userId": 123
  }
}
```

If developer / user opens the Url found in `type` property, they'll be able to see the information about the error. With the provided information, the developer can identify the issue and fix the issue easier. For the user, they can use this to seek support from developer.

## Example

Given the error object defined as below:

```csharp


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

visiting the URL in `type` property will see an error page like following:

> # User Not Found Error
> 
> ### Description
> 
> The error represents the requested object of type `User` could not be found
> 
> ### Reason
> 
> The error is thrown because the requested resource of type `User` could not be found
> 
> ### Response HTTP Status Code
> 
> The request that encounters this error shall be responded with HTTP Status Code 404.
> > Additionally, refer to [RFC 9110 Document Specification for Status Code 404](https:/datatracker.ietf.org/doc/html/rfc9110/#name-404-not-found).
> ### Response Body
> 
> Below is the sample response body of the request that encounters this error. Depends on the error, there can be additional properties that help you identify the issue.
> ```json
> {
>   "type": "https://localhost:7290/errors#WebAppCommonShared.Demo.Controllers.UserNotFoundError",
>   "title": "User Not Found Error",
>   "status": 404,
>   "detail": "The error is thrown because the requested resource of type User could not be> >found",
>   "instance": "{usually is URL of the request}"
> }
> ```

The `Description` of the page is pulled from XML Document `<summary>` tag, and the `Reason` is pulled from `<remarks>` tag. The `Response HTTP Status Code` is the status code that the error object is associated with. The `Response Body` is the sample response body that the error object will be returned.
