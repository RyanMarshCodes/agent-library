# Unit Testing Standards

Language-agnostic unit testing guidelines.

---

## Coverage Requirements

- **Target >90% code coverage** for all new code
- Coverage is a metric, not a goal - write meaningful tests, not just to hit percentages
- Prioritize testing:
  - Business logic and critical paths
  - Edge cases and error conditions
  - Complex conditional logic
  - Data transformations

---

## Test Structure

### Arrange-Act-Assert (AAA)

```csharp
[Fact]
public void CalculateTotal_WithMultipleItems_ReturnsSum()
{
    // Arrange
    var cart = new ShoppingCart();
    cart.AddItem(new Item("Widget", 10.00m, 2));
    cart.AddItem(new Item("Gadget", 15.00m, 1));

    // Act
    var total = cart.CalculateTotal();

    // Assert
    Assert.Equal(35.00m, total);
}
```

---

## Test Naming

Use descriptive names that explain the scenario and expected outcome:

```
[MethodName]_[Scenario]_[ExpectedResult]
```

Examples:
- `AddItem_WithValidQuantity_IncreasesItemCount`
- `ProcessPayment_WithInsufficientFunds_ThrowsException`
- `ParseDate_WithInvalidFormat_ReturnsNull`

---

## Dos

### Do: Test One Thing Per Test
```csharp
// Good - separate tests
[Fact]
public void AddItem_WithValidQuantity_AddsToCart() { }

[Fact]
public void AddItem_WithZeroQuantity_DoesNotAdd() { }

// Bad - testing multiple things
[Fact]
public void AddItem_ValidatesAndAddsAndNotifies() { }
```

### Do: Use Meaningful Assertions
```csharp
// Good - specific assertions
Assert.Equal(5, result.Count);
Assert.True(result.IsValid);
Assert.Contains("error", exception.Message);

// Bad - weak assertions
Assert.NotNull(result);
Assert.True(result.Count > 0);
```

### Do: Test Edge Cases
- Empty collections
- Null/None values
- Boundary conditions (0, -1, max values)
- Duplicate entries
- Concurrent access

### Do: Isolate Unit Under Test
- Use mocks/fakes for dependencies
- Avoid testing multiple components together
- Don't rely on external services or databases

### Do: Keep Tests Fast
- Unit tests should run in milliseconds
- Mock I/O operations
- Avoid sleep statements

### Do: Make Tests Deterministic
- No random values without seeded generators
- No reliance on current time/date (use injectable clocks)
- No network dependencies

---

## Don'ts

### Don't: Test Private Methods Directly
- Test through public API
- If private logic is complex, consider extracting to a separate class

### Don't: Use Magic Numbers
```csharp
// Bad
if (age > 18) { }

// Good
private const int LegalAge = 18;
if (age > LegalAge) { }
```

### Don't: Assert on Implementation Details
```csharp
// Bad - testing internal state
Assert.Equal(2, sut._items.Count);

// Good - testing behavior
Assert.Equal(2, cart.ItemCount);
```

### Don't: Share State Between Tests
- Each test should be independent
- Use setup/teardown for fresh instances
- Avoid static state

### Don't: Write Tests That Only Pass Sometimes
- Flaky tests erode trust in the test suite
- Remove timing dependencies
- Mock external systems

---

## Test Organization

### By Feature
```
/Tests
  /Features
    /Orders
      OrderServiceTests.cs
      OrderProcessorTests.cs
    /Customers
      CustomerServiceTests.cs
```

### By Type
```
/Tests
  /Unit
    /Services
      OrderServiceTests.cs
  /Integration
    /Api
      OrdersControllerTests.cs
```

---

## Mocking Guidelines

### Use Interfaces for Dependencies
```csharp
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int id);
}

public class OrderService
{
    private readonly IOrderRepository _repository;
    public OrderService(IOrderRepository repository) => _repository = repository;
}
```

### Mock Libraries
- **C#**: Moq, NSubstitute, FakeItEasy
- **JavaScript/TypeScript**: Jest mocks, Sinon
- **Python**: unittest.mock, pytest-mock

### Don't Overmock
```csharp
// Bad - mocking simple value objects
var mockDateTime = new Mock<IDateTime>();
mockDateTime.Setup(x => x.Now).Returns(new DateTime(2024, 1, 1));

// Good - inject concrete date/time provider
public interface ITimeProvider
{
    DateTime Now { get; }
}
```

---

## Utility Classes and Test Helpers

### Common Test Data
```csharp
public static class TestData
{
    public static Order CreateValidOrder() => new Order
    {
        Id = 1,
        CustomerId = 100,
        Items = [new OrderItem("Widget", 10.00m, 2)],
        Status = OrderStatus.Pending
    };

    public static Customer CreateCustomer(string name = "Test") => new Customer
    {
        Id = 1,
        Name = name,
        Email = "test@example.com"
    };
}
```

### Shared Fixtures
```csharp
public class DatabaseFixture : IDisposable
{
    public TestDbContext CreateContext() { /* ... */ }
    
    public void Dispose() { /* cleanup */ }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database")]
public class OrderRepositoryTests { }
```

---

## Teardown

### Per-Test Cleanup
- Dispose of resources in test
- Use `using` statements
- Clean up temporary files

```csharp
[Fact]
public void ProcessFile_CreatesOutputFile()
{
    var tempFile = Path.GetTempFileName();
    try
    {
        var processor = new FileProcessor();
        processor.Process("input.txt", tempFile);
        
        Assert.True(File.Exists(tempFile));
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
```

### Better: IDisposable Pattern
```csharp
public class TempFileTests : IDisposable
{
    private readonly string _tempFile;
    
    public TempFileTests()
    {
        _tempFile = Path.GetTempFileName();
    }
    
    [Fact]
    public void Test() { /* use _tempFile */ }
    
    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }
}
```

---

## Integration vs Unit Tests

### Unit Tests
- Test single class/method in isolation
- Fast (milliseconds)
- No external dependencies
- Use mocks

### Integration Tests
- Test multiple components together
- Slower (seconds)
- May use real database, file system
- Test actual flows

### When to Use Each
- **Unit**: Business logic, calculations, validation
- **Integration**: Database operations, file I/O, API calls, workflows

---

## Continuous Integration

- Run all tests on every commit
- Fail the build if tests fail
- Maintain fast test execution (< 5 minutes for full suite)
- Generate coverage reports
- Set coverage thresholds in CI

---

## Code Coverage Analysis

### What to Cover
- Public API surface
- Business logic
- Error handling paths
- Conditional branches

### What May Not Need Coverage
- Simple auto-properties
- DTOs/POCOs
- Generated code
- Code that wraps external libraries (mock those instead)
