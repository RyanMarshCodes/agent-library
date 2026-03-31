# C# Naming Conventions

Based on Microsoft [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines) and [Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

---

## General Rules

### PascalCase
Use PascalCase for:
- Class names
- Interface names
- Struct names
- Delegate names
- Method names
- Property names
- Event names
- Namespace names
- Public members
- Constant names (fields and local constants)

```csharp
public class CustomerService { }
public interface IWorkerQueue { }
public struct Point { }
public delegate void EventHandler(object sender, EventArgs e);
public void ProcessOrder() { }
public string CustomerName { get; set; }
public event EventHandler OrderProcessed;
```

### camelCase
Use camelCase for:
- Method arguments
- Local variables
- Private fields (with `_` prefix)

```csharp
public void ProcessOrder(OrderRequest request)
{
    var orderDetails = request.Details;
    var _internalCache = new Dictionary<string, object>();
}
```

---

## Special Naming Rules

### Interfaces
Prefix with `I` capital letter:
```csharp
public interface IEnumerable { }
public interface IWorkerQueue { }
public interface IConfigurationProvider { }
```

### Attributes
Suffix with `Attribute`:
```csharp
public class SerializableAttribute : Attribute { }
public class XmlElementAttribute : Attribute { }
```

### Enums
- **Non-flags**: Use singular nouns
- **Flags**: Use plural nouns

```csharp
public enum HttpStatusCode { OK, NotFound, Error }
[Flags]
public enum FileModes { Read, Write, Execute }
```

### Type Parameters
Use descriptive names, or single letter `T` if self-explanatory:
```csharp
public class Dictionary<TKey, TValue> { }
public interface IRepository<T> where T : class { }
public class Func<T, TResult> { }
```

### Static Fields
Prefix with `s_`:
```csharp
private static s_instance;
private static readonly s_defaultSettings = new Settings();
```

---

## Naming Avoidances

### DO NOT Use
- **Underscores** in identifiers (except private fields with `_` prefix)
- **Hungarian notation** (e.g., `strName`, `iCount`)
- **Abbreviations** unless widely accepted (e.g., `GetWindow`, not `GetWin`)
- **Keywords** as names (use `@` prefix if unavoidable: `@class`)
- **Two consecutive underscores** (`__`) - reserved for compiler

### Prefer Clarity Over Brevity
```csharp
// Good
public bool CanScrollHorizontally { get; }
public string CustomerFullName { get; }

// Avoid
public bool ScrollableX { get; }  // What is X?
public string CustName { get; }   // Ambiguous
```

---

## Word Choice

### DO
- Choose easily readable identifier names
- Use meaningful and descriptive names

### Avoid
- Cryptic abbreviations
- Single letters except for simple loop counters (`i`, `j`, `k`)
- Language-specific type names in parameters (e.g., `string` parameter named `string`)

---

## Assembly and Namespace Naming

### Assemblies
Name assemblies that represent their primary purpose:
```
Contoso.Commerce.dll
Contoso Finance.Core.dll
```

### Namespaces
Use reverse domain name notation:
```csharp
namespace Contoso.Commerce.OrderProcessing { }
namespace Contoso.Finance.Payroll { }
```
