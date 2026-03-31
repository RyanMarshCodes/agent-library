# C# Logging Standards

Based on Microsoft [Logging Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) and [ILogger](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger) best practices.

---

## Log Levels

| Level | Use For |
|-------|---------|
| **Trace** | Detailed diagnostic information, enter/exit methods |
| **Debug** | Debugging information, variable values during development |
| **Information** | General application events, important milestones |
| **Warning** | Recoverable issues, unexpected but handled situations |
| **Error** | Non-recoverable errors, exceptions |
| **Critical** | System-level failures, data corruption risk |

---

## Log Templates

### Use Message Templates (Not String Interpolation)
```csharp
// Good - structured logging
_logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", 
    order.Id, order.CustomerId);

// Bad - string interpolation
_logger.LogInformation($"Processing order {order.Id} for customer {order.CustomerId}");
```

### Include Relevant Context
```csharp
_logger.LogInformation(
    "Order {OrderId} created with {ItemCount} items, total {Total:C}",
    order.Id, order.Items.Count, order.Total);

_logger.LogError(ex,
    "Failed to process payment for order {OrderId}, amount {Amount}",
    order.Id, order.Total);
```

---

## What to Log

### Startup and Shutdown
```csharp
_logger.LogInformation("Application starting up at {StartTime}", DateTime.UtcNow);

public void Dispose()
{
    _logger.LogInformation("OrderService disposed");
}
```

### Business Events
```csharp
_logger.LogInformation(
    "Order {OrderId} placed by customer {CustomerId}, total: {Total:C}",
    order.Id, order.CustomerId, order.Total);

_logger.LogWarning(
    "Order {OrderId} flagged for manual review due to amount {Amount}",
    order.Id, order.Total);
```

### Security Events
```csharp
_logger.LogWarning("Failed login attempt for user {Username} from IP {IPAddress}",
    username, httpContext.Connection.RemoteIpAddress);

_logger.LogInformation("User {UserId} logged out", userId);
```

---

## What NOT to Log

### Never Log Secrets
```csharp
// Bad
_logger.LogInformation("API call with key {ApiKey}", apiKey);
_logger.LogInformation("Password reset for {Password}", password);

// Good
_logger.LogInformation("API call initiated");
```

### Never Log Sensitive Data
- Passwords, tokens, API keys
- Credit card numbers
- Social security numbers
- Personal health information (PHI)
- Full request/response bodies with PII

---

## Exception Logging

### Log with Context
```csharp
try
{
    await ProcessOrderAsync(order);
}
catch (OrderProcessingException ex)
{
    _logger.LogError(ex,
        "Failed to process order {OrderId}. Customer: {CustomerId}, Items: {ItemCount}",
        order.Id, order.CustomerId, order.Items.Count);
    throw;
}
```

### Don't Log and Rethrow
```csharp
// Bad - double logging
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    throw ex;
}

// Good - single log
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    throw;
}
```

---

## Performance Logging

### Log Long Operations
```csharp
var sw = Stopwatch.StartNew();
try
{
    await ProcessLargeFileAsync(filePath);
}
finally
{
    sw.Stop();
    _logger.LogInformation(
        "Processed file {FileName} in {ElapsedMs}ms",
        fileName, sw.ElapsedMilliseconds);
}
```

### Use Metrics for Timing
```csharp
using (_logger.BeginScope("Processing batch"))
{
    foreach (var item in items)
    {
        await ProcessAsync(item);
    }
}
```

---

## Structured Logging with Serilog

### Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Application", "OrderService")
    .CreateLogger();
```

### Enrichers
```csharp
.Enrich.WithMachineName()
.Enrich.WithEnvironmentUserName()
.Enrich.FromLogContext()
.Enrich.WithProperty("Version", "1.0.0")
```

---

## Best Practices

### Use Appropriate Levels
```csharp
// Trace - method entry/exit
_logger.LogTrace("Entering method {MethodName} with parameters {@Params}", 
    nameof(ProcessOrder), parameters);

// Debug - detailed debugging info
_logger.LogDebug("Processing item {ItemId}, current count: {Count}", 
    item.Id, items.Count);

// Information - business milestones
_logger.LogInformation("Order {OrderId} created", order.Id);

// Warning - recoverable issues
_logger.LogWarning("Retry attempt {AttemptNumber} for order {OrderId}",
    attempt, order.Id);

// Error - handled errors
_logger.LogError(ex, "Failed to send notification for order {OrderId}", order.Id);

// Critical - system failures
_logger.LogCritical(ex, "Database connection lost, shutting down");
```

### Avoid Logging in Hot Paths
```csharp
// Bad - logging on every iteration
foreach (var item in thousandsOfItems)
{
    _logger.LogDebug("Processing item {ItemId}", item.Id);  // Too much!
    await ProcessAsync(item);
}

// Better
foreach (var item in items)
{
    await ProcessAsync(item);
}
if (_logger.IsEnabled(LogLevel.Debug))
{
    _logger.LogDebug("Processed {Count} items", items.Count);
}
```

---

## ILogger Injection

### Constructor Injection
```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

### Generic Logger
```csharp
// For static/universal loggers
public static class AppLogger
{
    private static ILogger? _logger;
    
    public static void Initialize(ILoggerFactory factory)
    {
        _logger = factory.CreateLogger("App");
    }
}
```
