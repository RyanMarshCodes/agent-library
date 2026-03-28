# C# Async Programming

Based on Microsoft [async/await best practices](https://learn.microsoft.com/en-us/dotnet/csharp/async) and [ConfigureAwait FAQ](https://devblogs.microsoft.com/pfxteam/configureawait-faq/).

---

## Core Principles

### Use for I/O-Bound Operations
- Database calls
- Web API requests
- File operations
- Network requests

### Do Not Use for CPU-Bound Work
- Use `Task.Run()` for CPU-intensive work on background threads

---

## Async All the Way

Propagate async through the entire call stack:
```csharp
// Good - async all the way
public async Task<Order> GetOrderAsync(int id)
{
    return await _repository.GetOrderAsync(id);
}

// Bad - blocking the thread
public Order GetOrder(int id)
{
    return _repository.GetOrderAsync(id).GetAwaiter().GetResult();
}
```

---

## Return Types

### Task and Task<T>
Standard return types for async methods:
```csharp
public async Task<Order> GetOrderAsync(int id)
{
    var order = await _repository.FindAsync(id);
    return order;
}

public async Task SaveOrderAsync(Order order)
{
    await _repository.SaveAsync(order);
}
```

### ValueTask<T>
Use only for hot paths with frequent synchronous completions:
```csharp
// Only when the method often completes synchronously
public ValueTask<Order> GetOrderAsync(int id)
{
    if (_cache.TryGet(id, out var order))
        return ValueTask.FromResult(order);
    
    return new ValueTask<Order>(_repository.GetOrderAsync(id));
}
```

### IAsyncEnumerable<T>
For async streams:
```csharp
public async IAsyncEnumerable<Product> GetProductsAsync([EnumeratorCancellation] CancellationToken ct = default)
{
    foreach (var product in _products)
    {
        await Task.Delay(100, ct);
        yield return product;
    }
}
```

---

## Avoid Async Void

### Never Use Except for Event Handlers
```csharp
// Bad
public async void ProcessData() { }  // Exceptions cannot be caught!

// Good - for event handlers only
private async void Button_Click(object sender, EventArgs e)
{
    await ProcessAsync();
}
```

### Use async Task Instead
```csharp
public async Task ProcessDataAsync()
{
    await Task.Delay(100);
}
```

---

## ConfigureAwait

### Library Code
Use `ConfigureAwait(false)` to avoid capturing synchronization context:
```csharp
public async Task<Order> GetOrderAsync(int id)
{
    var order = await _repository.FindAsync(id).ConfigureAwait(false);
    return order;
}
```

### Application Code
- **ASP.NET Core**: No synchronization context - generally not needed
- **UI Applications**: Use when you don't need the UI context

### Rule of Thumb
Use `ConfigureAwait(false)` in library code; omit in application code unless specific context is needed.

---

## Cancellation

### Accept CancellationToken
```csharp
public async Task<Order> GetOrderAsync(int id, CancellationToken ct = default)
{
    // Check at appropriate points
    ct.ThrowIfCancellationRequested();
    
    var order = await _repository.FindAsync(id, ct);
    return order;
}
```

### Use Linked Tokens for Timeouts
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    await ProcessAsync(cts.Token);
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    // Handle timeout
}
```

---

## Exception Handling

### Always Await Within Try-Catch
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
        // Handle specific exceptions
        _logger.LogError(ex, "HTTP error occurred");
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error");
        throw;
    }
}
```

### Avoid Fire-and-Forget
```csharp
// Bad - exceptions are lost
_ = Task.Run(() => DoWork());

// Good - handle appropriately
_ = Task.Run(() => DoWork()).ContinueWith(t => 
    _logger.LogError(t.Exception, "Background work failed"), 
    TaskContinuationOptions.OnlyOnFaulted);
```

---

## Task.WhenAll vs Parallel.ForEachAsync

### WhenAll for Multiple Independent Tasks
```csharp
var tasks = orders.Select(o => ProcessOrderAsync(o));
await Task.WhenAll(tasks);
```

### Parallel.ForEachAsync for CPU-Bound Work
```csharp
await Parallel.ForEachAsync(items, new ParallelOptions { MaxDegreeOfParallelism = 4 },
    async (item, ct) =>
    {
        await ProcessItemAsync(item);
    });
```

---

## Common Pitfalls

### Blocking on Async Code
```csharp
// Bad - causes deadlock potential
var result = task.Result;

// Good
var result = await task;
```

### Not Awaiting Task
```csharp
// Bad - fire and forget in non-trivial scenarios
GetDataAsync();

// Good
await GetDataAsync();
```

### Async Without Await
```csharp
// Bad - compiler warning, does not run asynchronously
public async Task DoSomethingAsync()
{
    // No await - runs synchronously!
    var data = LoadData();
}
```

---

## Performance Tips

### Avoid Unnecessary Allocations
- Use `ValueTask<T>` for hot paths
- Reuse `HttpClient` instances
- Use `ArrayPool<T>` for large buffers

### Avoid Chatty Awaits
```csharp
// Chatty - many context switches
var a = await GetAAsync();
var b = await GetBAsync();
var c = await GetCAsync();

// Better - single await when possible
var (a, b, c) = await GetAllAsync();
```
