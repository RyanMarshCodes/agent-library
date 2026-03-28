# C# Error Handling

Based on Microsoft [Exception Handling](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/) guidelines.

---

## Throw Specific Exceptions

### Use Meaningful Exception Types
```csharp
// Good
ArgumentNullException.ThrowIfNull(order);
throw new InvalidOperationException("Order cannot be processed in current state");
throw new ArgumentException("Invalid order status", nameof(status));

// Bad
throw new Exception("Error");
throw new Error("Error");
```

### Common Exception Types
| Exception | Use For |
|-----------|--------|
| `ArgumentNullException` | Null argument passed to method |
| `ArgumentException` | Invalid argument value |
| `ArgumentOutOfRangeException` | Argument outside valid range |
| `InvalidOperationException` | Object state doesn't permit operation |
| `NotSupportedException` | Method/operation not supported |
| `NotImplementedException` | Feature not yet implemented |
| `ObjectDisposedException` | Using disposed object |
| `TimeoutException` | Operation timed out |

---

## Guard Clauses

### Validate Parameters Early
```csharp
public void ProcessOrder(Order order, int priority)
{
    ArgumentNullException.ThrowIfNull(order);
    
    if (priority < 0 || priority > 10)
        throw new ArgumentOutOfRangeException(nameof(priority));
        
    // Main logic
}
```

### Use Guard Clauses for Early Returns
```csharp
public void Process(Order order)
{
    // Guard clauses - fail fast
    if (order is null)
        throw new ArgumentNullException(nameof(order));
        
    if (!order.IsValid)
        throw new ArgumentException("Order is not valid", nameof(order));
        
    // Main logic
}
```

---

## Try-Catch Best Practices

### Catch Specific Exceptions
```csharp
try
{
    await _repository.SaveAsync(order);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database error saving order {OrderId}", order.Id);
    throw new DataException("Failed to save order", ex);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error saving order {OrderId}", order.Id);
    throw;
}
```

### Don't Catch What You Can't Handle
```csharp
// Bad - catching everything
try { }
catch (Exception ex) { /* ignore */ }

// Good - catch specific or rethrow
try { }
catch (SpecificException) { /* handle */ }
catch (Exception ex) { _logger.LogError(ex); throw; }
```

---

## Rethrowing Exceptions

### Use throw; Not throw ex;
```csharp
// Good - preserves stack trace
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing order");
    throw;
}

// Bad - loses stack trace
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing order");
    throw ex;
}
```

### Exception Filters (C# 6+)
```csharp
catch (Exception ex) when (ex is TimeoutException or OperationCanceledException)
{
    _logger.LogWarning("Operation cancelled or timed out");
}
```

---

## Async Exception Handling

### Handle Exceptions in Async Methods
```csharp
public async Task ProcessAsync()
{
    try
    {
        var result = await _service.GetDataAsync();
        // Process result
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP error");
        throw;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        _logger.LogError(ex, "Unexpected error");
        throw;
    }
}
```

### Avoid Fire and Forget
```csharp
// Bad - exceptions are lost
_ = Task.Run(() => DoWorkAsync());

// Good - handle or propagate
try
{
    await Task.Run(() => DoWorkAsync());
}
catch (Exception ex)
{
    _logger.LogError(ex, "Background work failed");
    throw;
}
```

---

## Exception Propagation

### Don't Swallow Exceptions
```csharp
// Bad
try { }
catch (Exception) { }  // Silent failure!

// Bad
try { }
catch (Exception ex)
{
    Log(ex.Message);  // Lose context
}

// Good
try { }
catch (SpecificException ex)
{
    // Handle specific case
}
catch (Exception ex)
{
    _logger.LogError(ex, "Context for debugging");
    throw;
}
```

---

## Finally and Using

### Use Using for Disposable Resources
```csharp
// Good - automatic disposal
using var reader = new StreamReader(path);
var content = await reader.ReadToEndAsync();

// Equivalent to:
StreamReader reader = null;
try
{
    reader = new StreamReader(path);
    // use
}
finally
{
    reader?.Dispose();
}
```

---

## Custom Exceptions

### Follow Naming Convention
```csharp
[Serializable]
public class OrderProcessingException : Exception
{
    public int OrderId { get; }
    
    public OrderProcessingException() { }
    
    public OrderProcessingException(string message) : base(message) { }
    
    public OrderProcessingException(string message, Exception inner) : base(message, inner) { }
    
    public OrderProcessingException(string message, int orderId) 
        : this(message) => OrderId = orderId;
        
    protected OrderProcessingException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
        OrderId = info.GetInt32(nameof(OrderId));
    }
    
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(OrderId), OrderId);
    }
}
```

---

## Logging and Exceptions

### Log at Appropriate Level
```csharp
try
{
    // Risky operation
}
catch (ArgumentNullException ex)
{
    _logger.LogDebug(ex, "Null argument - expected validation failure");
}
catch (TimeoutException ex)
{
    _logger.LogWarning(ex, "Operation timed out");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error during operation");
    throw;
}
```

### Include Context
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, 
        "Failed to process order {OrderId} for customer {CustomerId}", 
        order.Id, order.CustomerId);
    throw;
}
```

---

## Performance Considerations

### Avoid Exception for Control Flow
```csharp
// Bad - expensive
int GetValue(Dictionary<string, int> dict, string key)
{
    try
    {
        return dict[key];
    }
    catch (KeyNotFoundException)
    {
        return -1;
    }
}

// Good - use TryGetValue
int GetValue(Dictionary<string, int> dict, string key)
{
    return dict.TryGetValue(key, out var value) ? value : -1;
}
```

### Pre-validate When Performance Matters
```csharp
// For hot paths, check before accessing
if (index < 0 || index >= items.Count)
    throw new ArgumentOutOfRangeException(nameof(index));
    
var item = items[index];  // Already validated
```
