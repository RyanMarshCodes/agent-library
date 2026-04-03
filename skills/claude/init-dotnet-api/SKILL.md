---
name: init-dotnet-api
description: Scaffold an opinionated .NET 9 Web API with Clean Architecture, Azure-ready config, Docker Compose, and full quality tooling
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade .NET 9 Web API project. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## Inputs

Project name: $ARGUMENTS
(If no name provided, ask once then proceed.)

---

## Architecture: Clean Architecture (4-project solution)

```
{Name}/
├── {Name}.sln
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── docker-compose.yml
├── docker-compose.override.yml
├── README.md
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── {Name}.Domain/
│   ├── {Name}.Application/
│   ├── {Name}.Infrastructure/
│   └── {Name}.Api/
├── tests/
│   ├── {Name}.Domain.Tests/
│   ├── {Name}.Application.Tests/
│   ├── {Name}.Infrastructure.Tests/
│   └── {Name}.Api.Tests/
└── AppHost/
    └── {Name}.AppHost/          ← .NET Aspire
```

---

## Step 1: Initialize Solution

```bash
mkdir {Name} && cd {Name}
dotnet new sln -n {Name}

# Source projects
dotnet new classlib -n {Name}.Domain -o src/{Name}.Domain --framework net9.0
dotnet new classlib -n {Name}.Application -o src/{Name}.Application --framework net9.0
dotnet new classlib -n {Name}.Infrastructure -o src/{Name}.Infrastructure --framework net9.0
dotnet new webapi -n {Name}.Api -o src/{Name}.Api --framework net9.0 --use-minimal-apis

# Test projects
dotnet new xunit -n {Name}.Domain.Tests -o tests/{Name}.Domain.Tests --framework net9.0
dotnet new xunit -n {Name}.Application.Tests -o tests/{Name}.Application.Tests --framework net9.0
dotnet new xunit -n {Name}.Infrastructure.Tests -o tests/{Name}.Infrastructure.Tests --framework net9.0
dotnet new xunit -n {Name}.Api.Tests -o tests/{Name}.Api.Tests --framework net9.0

# Aspire AppHost
dotnet new aspire-apphost -n {Name}.AppHost -o AppHost/{Name}.AppHost

# Add to solution
dotnet sln add src/{Name}.Domain/{Name}.Domain.csproj
dotnet sln add src/{Name}.Application/{Name}.Application.csproj
dotnet sln add src/{Name}.Infrastructure/{Name}.Infrastructure.csproj
dotnet sln add src/{Name}.Api/{Name}.Api.csproj
dotnet sln add tests/{Name}.Domain.Tests/{Name}.Domain.Tests.csproj
dotnet sln add tests/{Name}.Application.Tests/{Name}.Application.Tests.csproj
dotnet sln add tests/{Name}.Infrastructure.Tests/{Name}.Infrastructure.Tests.csproj
dotnet sln add tests/{Name}.Api.Tests/{Name}.Api.Tests.csproj
dotnet sln add AppHost/{Name}.AppHost/{Name}.AppHost.csproj
```

---

## Step 2: Directory.Build.props

Create at solution root — applies to ALL projects:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisMode>Recommended</AnalysisMode>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <Optimize>true</Optimize>
  </PropertyGroup>

  <!-- Static Analysis Analyzers -->
  <ItemGroup>
    <PackageReference Include="Meziantou.Analyzer" Version="*" PrivateAssets="all" />
    <PackageReference Include="Roslynator.Analyzers" Version="*" PrivateAssets="all" />
    <PackageReference Include="SonarAnalyzer.CSharp" Version="*" PrivateAssets="all" />
  </ItemGroup>

  <!-- Baseline legacy warnings: uncomment after initial build shows warnings -->
  <!-- <NoWarn>$(NoWarn);CA0000</NoWarn> -->
</Project>
```

---

## Step 3: Directory.Packages.props (Central Package Management)

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup Label="Application">
    <PackageVersion Include="MediatR" Version="12.*" />
    <PackageVersion Include="FluentValidation" Version="11.*" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
    <PackageVersion Include="Ardalis.Result" Version="9.*" />
    <PackageVersion Include="Ardalis.GuardClauses" Version="4.*" />
  </ItemGroup>

  <ItemGroup Label="Infrastructure">
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.*" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.*" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="9.*" />
    <PackageVersion Include="Azure.Identity" Version="1.*" />
    <PackageVersion Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.*" />
    <PackageVersion Include="Azure.Storage.Blobs" Version="12.*" />
    <PackageVersion Include="Azure.Messaging.ServiceBus" Version="7.*" />
  </ItemGroup>

  <ItemGroup Label="API">
    <PackageVersion Include="Scalar.AspNetCore" Version="2.*" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="9.*" />
    <PackageVersion Include="Serilog.AspNetCore" Version="8.*" />
    <PackageVersion Include="Serilog.Sinks.ApplicationInsights" Version="4.*" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="6.*" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.*" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.*" />
    <PackageVersion Include="OpenTelemetry.Exporter.AzureMonitor" Version="1.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />
  </ItemGroup>

  <ItemGroup Label="Testing">
    <PackageVersion Include="xunit" Version="2.*" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageVersion Include="FluentAssertions" Version="7.*" />
    <PackageVersion Include="NSubstitute" Version="5.*" />
    <PackageVersion Include="Bogus" Version="35.*" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.*" />
    <PackageVersion Include="Testcontainers.MsSql" Version="3.*" />
    <PackageVersion Include="coverlet.collector" Version="6.*" />
  </ItemGroup>
</Project>
```

---

## Step 4: .editorconfig

```ini
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{cs,csx}]
# Naming rules
dotnet_naming_rule.interface_should_be_begins_with_i.severity = error
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_symbols.interface.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected

dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case

# Private fields: _camelCase
dotnet_naming_rule.private_fields_should_be_camel_case.severity = error
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = camel_case_with_underscore

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.camel_case_with_underscore.required_prefix = _
dotnet_naming_style.camel_case_with_underscore.capitalization = camel_case

# Code style
csharp_style_var_for_built_in_types = false:error
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion
csharp_prefer_braces = true:error
csharp_style_expression_bodied_methods = when_on_single_line:suggestion
csharp_style_pattern_matching_over_is_with_cast_check = true:error
csharp_style_pattern_matching_over_as_with_null_check = true:error
csharp_style_throw_expression = true:suggestion
csharp_style_conditional_delegate_call = true:error
dotnet_style_null_propagation = true:error
dotnet_style_object_initializer = true:suggestion
dotnet_style_collection_initializer = true:suggestion
dotnet_style_prefer_is_null_check_over_reference_equality_method = true:error
dotnet_style_explicit_tuple_names = true:error
dotnet_style_prefer_auto_properties = true:error
dotnet_style_require_accessibility_modifiers = always:error

[*.{json,yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

---

## Step 5: .gitignore

Use the standard .NET gitignore:
```bash
dotnet new gitignore
```

Add these additional entries at the bottom:
```
# User secrets
secrets.json
appsettings.*.local.json

# Docker
docker-compose.override.local.yml
```

---

## Step 6: Domain Project (`{Name}.Domain`)

Structure:
```
src/{Name}.Domain/
├── Common/
│   ├── Entity.cs
│   ├── ValueObject.cs
│   ├── AggregateRoot.cs
│   └── DomainEvent.cs
├── Interfaces/
│   └── IRepository.cs
└── GlobalUsings.cs
```

`Entity.cs`:
```csharp
namespace {Name}.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected void SetUpdated() => UpdatedAt = DateTime.UtcNow;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

`AggregateRoot.cs`:
```csharp
namespace {Name}.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

`DomainEvent.cs`:
```csharp
using MediatR;

namespace {Name}.Domain.Common;

public abstract record DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

`IRepository.cs`:
```csharp
using System.Linq.Expressions;

namespace {Name}.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
```

`GlobalUsings.cs`:
```csharp
global using {Name}.Domain.Common;
global using {Name}.Domain.Interfaces;
```

---

## Step 7: Application Project (`{Name}.Application`)

Add packages:
```bash
dotnet add src/{Name}.Application package MediatR
dotnet add src/{Name}.Application package FluentValidation
dotnet add src/{Name}.Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/{Name}.Application package Ardalis.Result
```

Add project reference:
```bash
dotnet add src/{Name}.Application reference src/{Name}.Domain
```

Structure:
```
src/{Name}.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── PerformanceBehavior.cs
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   └── NotFoundException.cs
│   └── Models/
│       └── PaginatedList.cs
├── DependencyInjection.cs
└── GlobalUsings.cs
```

`ValidationBehavior.cs`:
```csharp
using FluentValidation;
using MediatR;
using ValidationException = {Name}.Application.Common.Exceptions.ValidationException;

namespace {Name}.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
```

`LoggingBehavior.cs`:
```csharp
using MediatR;
using Microsoft.Extensions.Logging;

namespace {Name}.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(cancellationToken);

        logger.LogInformation("Handled {RequestName}", requestName);
        return response;
    }
}
```

`PerformanceBehavior.cs`:
```csharp
using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace {Name}.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        timer.Stop();

        if (timer.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "Slow request detected: {RequestName} took {ElapsedMs}ms",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds);
        }

        return response;
    }
}
```

`NotFoundException.cs`:
```csharp
namespace {Name}.Application.Common.Exceptions;

public sealed class NotFoundException(string name, object key)
    : Exception($"Entity '{name}' with key '{key}' was not found.");
```

`ValidationException.cs`:
```csharp
using FluentValidation.Results;

namespace {Name}.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
```

`DependencyInjection.cs`:
```csharp
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using {Name}.Application.Common.Behaviors;

namespace {Name}.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

---

## Step 8: Infrastructure Project (`{Name}.Infrastructure`)

Add packages:
```bash
dotnet add src/{Name}.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/{Name}.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/{Name}.Infrastructure package Azure.Identity
dotnet add src/{Name}.Infrastructure package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add src/{Name}.Infrastructure package Azure.Storage.Blobs
dotnet add src/{Name}.Infrastructure package Azure.Messaging.ServiceBus
```

Add project reference:
```bash
dotnet add src/{Name}.Infrastructure reference src/{Name}.Application
```

Structure:
```
src/{Name}.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Interceptors/
│   │   └── AuditableEntityInterceptor.cs
│   └── Repositories/
│       └── RepositoryBase.cs
├── DependencyInjection.cs
└── GlobalUsings.cs
```

`ApplicationDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;

namespace {Name}.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

`RepositoryBase.cs`:
```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using {Name}.Domain.Interfaces;

namespace {Name}.Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext Context = context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Context.Set<T>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Context.Set<T>().Where(predicate).ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Context.Set<T>().AnyAsync(predicate, cancellationToken);
}
```

`DependencyInjection.cs`:
```csharp
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using {Name}.Infrastructure.Persistence;

namespace {Name}.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        // Azure clients — use DefaultAzureCredential (Managed Identity in Azure, local dev otherwise)
        services.AddSingleton(_ =>
        {
            var blobUri = configuration["Azure:StorageAccount:Uri"]
                ?? throw new InvalidOperationException("Azure:StorageAccount:Uri not configured.");
            return new Azure.Storage.Blobs.BlobServiceClient(
                new Uri(blobUri),
                new DefaultAzureCredential());
        });

        return services;
    }
}
```

---

## Step 9: API Project (`{Name}.Api`)

Add packages:
```bash
dotnet add src/{Name}.Api package Scalar.AspNetCore
dotnet add src/{Name}.Api package Microsoft.AspNetCore.OpenApi
dotnet add src/{Name}.Api package Serilog.AspNetCore
dotnet add src/{Name}.Api package Serilog.Sinks.ApplicationInsights
dotnet add src/{Name}.Api package Serilog.Sinks.Console
dotnet add src/{Name}.Api package OpenTelemetry.Extensions.Hosting
dotnet add src/{Name}.Api package OpenTelemetry.Instrumentation.AspNetCore
dotnet add src/{Name}.Api package OpenTelemetry.Exporter.AzureMonitor
dotnet add src/{Name}.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

Add project references:
```bash
dotnet add src/{Name}.Api reference src/{Name}.Application
dotnet add src/{Name}.Api reference src/{Name}.Infrastructure
```

`Program.cs`:
```csharp
using Scalar.AspNetCore;
using Serilog;
using {Name}.Api.Middleware;
using {Name}.Application;
using {Name}.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Logging: Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation())
    .UseAzureMonitor();

// API
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
    }));

// Health checks
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<Infrastructure.Persistence.ApplicationDbContext>();

// Auth
builder.Services
    .AddAuthentication()
    .AddJwtBearer();

builder.Services.AddAuthorization();

// Application layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

// Health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Feature endpoints — register via extension methods
// app.MapFeatureEndpoints();

app.Run();

// Required for integration tests
public partial class Program { }
```

`appsettings.json`:
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.ApplicationInsights"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "connectionString": "",
          "telemetryConverter": "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Azure": {
    "StorageAccount": {
      "Uri": ""
    },
    "KeyVault": {
      "Uri": ""
    },
    "ApplicationInsights": {
      "ConnectionString": ""
    }
  },
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "Authority": "",
        "Audience": ""
      }
    }
  },
  "AllowedHosts": "*"
}
```

`appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database={Name};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true"
  }
}
```

---

## Step 10: Docker Compose

`docker-compose.yml`:
```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/{Name}.Api/Dockerfile
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080;https://+:8081
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - {name}-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1" -b -o /dev/null
      interval: 10s
      timeout: 3s
      retries: 10
      start_period: 10s
    networks:
      - {name}-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    networks:
      - {name}-network

volumes:
  sqlserver-data:

networks:
  {name}-network:
    driver: bridge
```

`docker-compose.override.yml`:
```yaml
services:
  api:
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database={Name};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true
```

`src/{Name}.Api/Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/{Name}.Api/{Name}.Api.csproj", "src/{Name}.Api/"]
COPY ["src/{Name}.Application/{Name}.Application.csproj", "src/{Name}.Application/"]
COPY ["src/{Name}.Infrastructure/{Name}.Infrastructure.csproj", "src/{Name}.Infrastructure/"]
COPY ["src/{Name}.Domain/{Name}.Domain.csproj", "src/{Name}.Domain/"]
RUN dotnet restore "src/{Name}.Api/{Name}.Api.csproj"

COPY . .
WORKDIR "/src/src/{Name}.Api"
RUN dotnet build "{Name}.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "{Name}.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "{Name}.Api.dll"]
```

---

## Step 11: GitHub Actions CI

`.github/workflows/ci.yml`:
```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOTNET_VERSION: '9.0.x'
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build-and-test:
    name: Build & Test
    runs-on: ubuntu-latest

    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong!Passw0rd
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -Q 'SELECT 1' -b -o /dev/null"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 10

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore -c Release

      - name: Test
        run: dotnet test --no-build -c Release --collect:"XPlat Code Coverage" --results-directory ./coverage

      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          directory: ./coverage
```

---

## Step 12: Testing Projects

Add packages to all test projects:
```bash
for proj in tests/{Name}.Domain.Tests tests/{Name}.Application.Tests tests/{Name}.Infrastructure.Tests tests/{Name}.Api.Tests; do
  dotnet add $proj package FluentAssertions
  dotnet add $proj package NSubstitute
  dotnet add $proj package Bogus
  dotnet add $proj package coverlet.collector
done
dotnet add tests/{Name}.Api.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/{Name}.Infrastructure.Tests package Testcontainers.MsSql
```

---

## Step 13: README.md

Create a complete README covering:
- Project purpose and architecture diagram (text-based)
- Prerequisites (SDK, Docker, Azure CLI)
- Local setup steps
- How to run with Docker Compose
- How to run with Aspire
- How to run tests
- Environment configuration table
- Azure deployment notes

---

## Final Checklist

- [ ] Solution builds with `dotnet build` — zero warnings, zero errors
- [ ] All 4 source projects exist with correct references
- [ ] All 4 test projects exist
- [ ] `Directory.Build.props` enforces nullable, TreatWarningsAsErrors
- [ ] `Directory.Packages.props` manages all package versions centrally
- [ ] `.editorconfig` configured with strict C# rules
- [ ] `Program.cs` has Serilog, OpenTelemetry, Problem Details, Rate Limiting, Health Checks
- [ ] `docker-compose.yml` includes api, sqlserver, redis
- [ ] `Dockerfile` is multi-stage
- [ ] GitHub Actions CI pipeline created
- [ ] `.gitignore` includes all standard .NET excludes
- [ ] `appsettings.json` has no hardcoded secrets
- [ ] `README.md` covers local setup end-to-end
