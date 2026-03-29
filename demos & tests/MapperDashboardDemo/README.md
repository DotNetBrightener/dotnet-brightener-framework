# DotNetBrightener Mapper Dashboard Demo

A demonstration WebAPI project showcasing the DotNetBrightener Mapper framework's capabilities, including automatic DTO generation, mapping extensions, and the interactive Mapper Dashboard.

## Overview

This demo project illustrates:
- **Automatic DTO Generation**: Using source generators to create mapping targets at compile time
- **Mapping Extensions**: `ToTarget()`, `SelectTargets()`, and `ToSource()` for easy object mapping
- **Mapper Dashboard**: Interactive web-based visualization of mapping configurations
- **Minimal APIs**: Clean, modern ASP.NET Core minimal API endpoints

## Project Structure

```
MapperDashboardDemo/
├── Entities/                 # Domain entities (User, Product, Order, etc.)
├── DtoTargets/              # Generated DTO targets
│   └── MappingConfigurations/  # Custom mapping configurations
├── Services/                # Application services
│   └── SeedDataService.cs   # Bogus-based test data generation
├── Endpoints/               # API endpoint definitions
│   ├── UserEndpoints.cs
│   ├── ProductEndpoints.cs
│   ├── CompanyEndpoints.cs
│   ├── OrderEndpoints.cs
│   └── BlogPostEndpoints.cs
└── Program.cs               # Application entry point
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Windows 11, Linux, or macOS

### Build and Run

```bash
cd "demos & tests/MapperDashboardDemo"

# Restore packages
dotnet restore

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run
```

The application will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## API Endpoints

### Users
- `GET /api/users` - List all users (UserListItemDto)
- `GET /api/users/{id}` - Get user details (UserDetailDto)
- `GET /api/users/{id}/summary` - Get user summary (UserSummaryDto)
- `GET /api/users/{id}/edit` - Get user for editing (UserEditDto)
- `POST /api/users/from-dto` - Convert DTO to source entity
- `GET /api/users/{id}/full-name` - Get user with full name (UserWithFullNameDto)

### Products
- `GET /api/products` - List all products (ProductDto)
- `GET /api/products/lookup` - Product lookup list (ProductLookupDto)
- `GET /api/products/query?name=&minPrice=` - Query products (ProductQueryDto)
- `GET /api/products/{id}/validated` - Get validated product (ProductValidatedDto)

### Companies
- `GET /api/companies` - List all companies (CompanyDto)
- `GET /api/companies/{id}` - Get company details (CompanyDto)

### Orders
- `GET /api/orders` - List all orders (OrderListDto)
- `GET /api/orders/{id}` - Get order details (OrderDto)

### Blog Posts
- `GET /api/blog-posts` - List all blog posts (raw entities)
- `GET /api/blog-posts/{id}` - Get blog post details

## Mapper Dashboard

Access the interactive Mapper Dashboard at:
```
https://localhost:5001/dnb-mapper
```

The dashboard provides:
- Visual representation of all mapping configurations
- Entity-to-DTO mapping visualization
- Interactive exploration of generated types
- JSON API for programmatic access

## Features Demonstrated

### 1. Automatic DTO Generation
DTO targets are automatically generated at compile time using source generators:
- No manual DTO class creation required
- Type-safe mapping with IntelliSense support
- Compile-time error detection

### 2. Mapping Extensions

#### ToTarget()
Map a single entity to its DTO target:
```csharp
var userDto = user.ToTarget<User, UserDetailDto>();
```

#### SelectTargets()
Map a collection of entities:
```csharp
var userDtos = users.SelectTargets<User, UserListItemDto>();
```

#### ToSource()
Map a DTO back to its source entity:
```csharp
var user = userEditDto.ToSource<UserEditDto, User>();
```

### 3. Nested Mapping
Complex nested objects are automatically mapped:
- User → UserProfile → SocialLinks
- Order → OrderItems → Product
- Company → Address

### 4. Custom Mapping Configurations
Specify custom property mappings in `DtoTargets/MappingConfigurations/`

## Test Data

The application uses **Bogus** library to generate realistic test data:
- 20 Users with profiles and social links
- 5 Companies with addresses
- 30 Products with tags
- 15 Orders with items and shipping addresses
- 10 Blog posts with tags

All data is generated with a fixed seed (42) for reproducibility.

## Swagger UI

Interactive API documentation available at:
```
https://localhost:5001/swagger
```

## Technologies

- **.NET 10.0** - Latest .NET platform
- **ASP.NET Core Minimal APIs** - Modern API development
- **DotNetBrightener.Mapper** - Automatic DTO generation and mapping
- **Bogus** - Test data generation
- **Swashbuckle** - Swagger/OpenAPI documentation

## License

This demo is part of the DotNetBrightener Framework project.
