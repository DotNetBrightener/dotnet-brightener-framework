# Quick Start Guide

This guide will help you get up and running with `MappingTarget<TSource>` in just a few steps.

## 1. Install the NuGet Package

```
dotnet add package DotNetBrightener.Mapper
```

For LINQ helpers:
```
dotnet add package DotNetBrightener.Mapper.Mapping
```

## 2. Define Your Source Model

```csharp
public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
```

## 3. Create a Target DTO / Projection

```csharp
using DotNetBrightener.Mapper.Attributes;

// Class
[MappingTarget<Person>(nameof(Person.Email))]
public partial class PersonDto { }

// Record (inferred from 'record' keyword)
[MappingTarget<Person>]
public partial record PersonDto { }

// Struct (inferred from 'struct' keyword)
[MappingTarget<Person>]
public partial struct PersonDto { }
```

## 4. Use the Generated Type

```csharp
var person = new Person { Name = "Alice", Email = "a@b.com", Age = 30 };

var dto = new PersonDto(person); // Uses generated constructor
```

## 5. LINQ Integration

```csharp
var query = dbContext.People.Select(PersonDto.Projection).ToList();
```

Or with DotNetBrightener.Mapper.Mapping:

```csharp
using DotNetBrightener.Mapper.Mapping;

var dto = person.ToTarget<PersonDto>();

var dtos = personList.SelectTargets<PersonDto>();
```

---

See the [Attribute Reference](03_AttributeReference.md) and [Extension Methods](05_Extensions.md) for more details.
