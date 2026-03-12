# Data Migration CLI Tool

&copy; 2025 [DotNet Brightener](mailto:admin@dotnetbrightener.com)

## Versions

| Package | Version |
| --- | --- |
| DotNetBrightener.DataAccess.DataMigration | ![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.DataAccess.DataMigration) |
| dotnet-dnb-datamigration | ![NuGet Version](https://img.shields.io/nuget/v/dotnet-dnb-datamigration) |


## Overview

This CLI tool is a .NET tool which you can call from command line. It helps you to create a new data migration class in your project in case you use [Data Migration](https://www.nuget.org/packages/DotNetBrightener.DataAccess.DataMigration) package. Follow instruction in that package to install it to your project, and use this CLI tool to create new migration class when needed.

## Installation

### Install using Package Reference
   
```bash
dotnet tool install --global dotnet-dnb-datamigration
```


### Usage

At the root folder of your project where you want to have data migration, run the following command:

```bash

dotnet dnb-datamigration add [migration_name]

```

A new migration class will be created in the `DataMigrations` folder.

You'll need to follow instructions from [Data Migration](https://www.nuget.org/packages/DotNetBrightener.DataAccess.DataMigration) package to implement your migration class.