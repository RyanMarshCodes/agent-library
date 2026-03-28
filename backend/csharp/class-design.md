# C# Class and Type Design

Based on Microsoft [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/).

---

## Class Design

### Single Responsibility
Each class should have one reason to change:
```csharp
// Good - focused responsibility
public class OrderValidator
{
    public bool Validate(Order order) { /* validation logic */ }
}

public class OrderRepository
{
    public Task<Order> GetByIdAsync(int id) { /* DB logic */ }
}

// Bad - too many responsibilities
public class OrderManager
{
    public bool Validate(Order order) { }      // Validation
    public Task SaveAsync(Order order) { }    // Persistence
    public void SendNotification(Order o) { } // Notification
}
```

### Dependency Injection
Use constructor injection:
```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _emailService = emailService;
        _logger = logger;
    }
}
```

---

## Property Design

### Auto-Properties
Use when no custom logic is needed:
```csharp
public string Name { get; set; }
public bool IsActive { get; private set; }
```

### Read-Only Properties
```csharp
public string Id { get; }
// Or with init (C# 9+)
public string Name { get; init; }
```

### Expression-Bodied for Simple Getters
```csharp
public string FullName => $"{FirstName} {LastName}";
public int Count => _items.Count;
```

### Guard Clauses in Setters
```csharp
private string _name;
public string Name
{
    get => _name;
    set => _name = value ?? throw new ArgumentNullException(nameof(value));
}
```

---

## Interface Design

### Keep Interfaces Small
Follow Interface Segregation Principle:
```csharp
// Good - focused interfaces
public interface IReadableRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWritableRepository<T>
{
    Task SaveAsync(T entity);
    Task DeleteAsync(int id);
}

// Bad - bloated interface
public interface IRepository<T>
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task SaveAsync(T entity);
    Task DeleteAsync(int id);
    // ... more methods
}
```

### Explicit Interface Implementation
Use when implementation detail should be hidden:
```csharp
public class OrderProcessor : IOrderProcessor, IShippingCalculator
{
    // Public - part of IOrderProcessor
    public void Process(Order order) { }

    // Explicit - part of IShippingCalculator
    decimal IShippingCalculator.CalculateShipping(Order order) { }
}
```

---

## Record Types

### Use for Immutable Data Carriers
```csharp
// Simple record
public record Person(string FirstName, string LastName);

// With additional members
public record Person(string FirstName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}";
    public int Age { get; init; }
}
```

### Value Equality
Records automatically implement value equality:
```csharp
var person1 = new Person("John", "Doe");
var person2 = new Person("John", "Doe");
Console.WriteLine(person1 == person2);  // True
```

---

## Struct Design

### Use for Small, Immutable Data
```csharp
public readonly struct Point
{
    public double X { get; }
    public double Y { get; }

    public Point(double x, double y) => (X, Y) = (x, y);
    
    public double DistanceTo(Point other) => 
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
}
```

### Use readonly for Immutable Structs
```csharp
public readonly struct Dimensions
{
    public double Width { get; }
    public double Height { get; }
    
    public Dimensions(double width, double height) => (Width, Height) = (width, height);
}
```

---

## Enum Design

### Non-Flags Enums
Use singular nouns:
```csharp
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

### Flags Enums
Use plural nouns and [Flags]:
```csharp
[Flags]
public enum FilePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    ReadWrite = Read | Write,
    All = Read | Write | Execute
}
```

### Avoid Enum Values Named "Reserved"
```csharp
// Bad
public enum Status
{
    Active,
    Reserved,  // Assumption: will be used later
    Inactive
}
```

---

## Field Design

### Private Fields
- Use `_camelCase` naming
- Use `readonly` when immutability is intended
- Avoid public fields

```csharp
private readonly IOrderRepository _repository;
private readonly ILogger _logger;
private int _instanceCount;
```

### Constants
Use `const` for compile-time constants:
```csharp
public const int MaxRetries = 3;
public const string DefaultCurrency = "USD";
```

---

## Method Design

### Keep Methods Focused
- Do one thing and do it well
- Aim for < 20 lines
- Use parameters to express dependencies

### Parameter Validation
```csharp
public void ProcessOrder(Order order, int priority)
{
    ArgumentNullException.ThrowIfNull(order);
    
    if (priority < 0 || priority > 10)
        throw new ArgumentOutOfRangeException(nameof(priority));
        
    // Process...
}
```

---

## Extension Methods

### Place in Static Classes
```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => 
        string.IsNullOrEmpty(value);
        
    public static string Truncate(this string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
```

---

## Sealed Classes

### Consider sealing to prevent inheritance when not designed for extension:
```csharp
public sealed class OrderProcessor
{
    // Implementation
}
```
