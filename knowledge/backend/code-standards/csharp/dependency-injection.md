# C# Dependency Injection

Based on Microsoft [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) guidelines.

---

## Constructor Injection

### Preferred Pattern
```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Null Checks
- Check for null on required dependencies
- Use `ArgumentNullException.ThrowIfNull()` (C# 10+)

---

## Interface Segregation

### Depend on Abstractions
```csharp
// Good - depend on interface
public class OrderService
{
    private readonly IReadOnlyOrderRepository _readOnlyRepo;
    private readonly IWriteOnlyOrderRepository _writeOnlyRepo;
    
    public OrderService(
        IReadOnlyOrderRepository readOnlyRepo,
        IWriteOnlyOrderRepository writeOnlyRepo)
    {
        _readOnlyRepo = readOnlyRepo;
        _writeOnlyRepo = writeOnlyRepo;
    }
}

// Bad - depend on concrete
public class OrderService
{
    private readonly SqlOrderRepository _repository;
}
```

---

## Service Lifetimes

### Transient
New instance every time:
```csharp
services.AddTransient<IOrderService, OrderService>();

// Use for: Lightweight, stateless services
```

### Scoped
One instance per scope (request in web apps):
```csharp
services.AddScoped<IOrderService, OrderService>();

// Use for: Database contexts, unit of work
```

### Singleton
One instance for application lifetime:
```csharp
services.AddSingleton<IConfiguration, Configuration>();

// Use for: Configuration, caching, loggers
```

---

## Registration Order

### Register Dependencies Before Consumers
```csharp
// 1. Register foundational services first
services.AddSingleton<IConfiguration>(Configuration);
services.AddLogging();

// 2. Register repositories
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<ICustomerRepository, CustomerRepository>();

// 3. Register services that depend on repositories
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<ICustomerService, CustomerService>();

// 4. Register controllers/handlers last
services.AddControllers();
```

---

## Taking Dependencies

### From HttpContext (ASP.NET Core)
```csharp
public class MyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public MyService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public void DoWork()
    {
        var userId = _httpContextAccessor.HttpContext?.User
            ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
```

### From Service Provider (Avoid When Possible)
```csharp
// Only when absolutely necessary
var scopedService = serviceProvider.GetRequiredService<IScopedService>();
```

---

## Factory Patterns

### For Complex Construction
```csharp
public interface IOrderProcessorFactory
{
    IOrderProcessor Create(ProcessOptions options);
}

public class OrderProcessorFactory : IOrderProcessorFactory
{
    public IOrderProcessor Create(ProcessOptions options)
    {
        return options.Type switch
        {
            ProcessType.Standard => new StandardOrderProcessor(),
            ProcessType.Express => new ExpressOrderProcessor(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
```

### For Multi-Tenant Scenarios
```csharp
services.AddScoped<ITenantService>(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>();
    var tenantId = httpContext.HttpContext?.Request.Headers["X-TenantId"];
    return new TenantService(tenantId);
});
```

---

## Testing with DI

### Mock Dependencies
```csharp
[Fact]
public void ProcessOrder_WithValidOrder_CallsRepository()
{
    // Arrange
    var mockRepo = new Mock<IOrderRepository>();
    var mockEmail = new Mock<IEmailService>();
    var mockLogger = new Mock<ILogger<OrderService>>();
    
    var service = new OrderService(
        mockRepo.Object,
        mockEmail.Object,
        mockLogger.Object);
        
    var order = new Order { Id = 1, Items = [new OrderItem()] };
    
    // Act
    service.ProcessOrder(order);
    
    // Assert
    mockRepo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
}
```

---

## Anti-Patterns to Avoid

### Don't Inject HttpContext into Singleton
```csharp
// Bad
public class SingletonService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public SingletonService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
}

// Better - inject service that resolves on demand
public class BetterService
{
    private readonly IServiceProvider _sp;
    
    public BetterService(IServiceProvider sp) => _sp = sp;
    
    public void DoWork()
    {
        using var scope = _sp.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
    }
}
```

### Don't Mix Lifetimes Unnecessarily
```csharp
// Bad - Scoped injected into Singleton can cause issues
public class SingletonService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    public SingletonService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;  // OK - factory pattern
    }
}
```

---

## Service Collection Extensions

### Organize Registration
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
    
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));
            
        return services;
    }
}
```

### Usage in Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();
```
