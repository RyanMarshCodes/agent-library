# C# Code Style

Based on Microsoft [Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

---

## Formatting

### Indentation
- Use 4 spaces for indentation
- No tabs
- Set `tab_width = 4`

### Line Endings
- Use `crlf` (Windows) or `lf` (cross-platform)
- Be consistent within the project

### New Lines
```csharp
// Place opening brace on new line for methods, properties, control blocks
void MyMethod()
{
    if (condition)
    {
        // code
    }
}

// New line before else, catch, finally
if (condition)
{
    // code
}
else
{
    // code
}

try
{
    // code
}
catch (Exception ex)
{
    // code
}
finally
{
    // code
}
```

### Spacing
- Use spaces around binary operators: `a + b`
- No space between method name and parenthesis: `Method()`
- Space after commas: `Method(a, b, c)`
- Space before colon in inheritance: `class C : I`
- Space after cast: `(string)value`

---

## Using Directives

### Placement
Place `using` directives outside the namespace declaration:
```csharp
using Azure;
using System.Collections.Generic;

namespace CoolStuff.AwesomeFeature
{
    // code
}
```

### Ordering
- Sort `System.*` directives first
- Then sort other directives alphabetically
- Separate with blank line between groups

### Global Using (C# 10+)
Use global usings for frequently used namespaces to reduce boilerplate:
```csharp
global using System;
global using System.Collections.Generic;
global using Microsoft.Extensions.Logging;
```

---

## Var Usage

### When to Use
Use `var` when the type is apparent from the right side:
```csharp
var list = new List<int>();
var customer = new Customer();
var settings = Options.Default;
```

### When to Avoid
Avoid `var` when the type is not obvious:
```csharp
// Hard to infer type - use explicit type
var dict = new Dictionary<string, List<int>>();

// Use explicit type in foreach
foreach (var item in collection)  // OK if type is obvious
foreach (KeyValuePair<string, object> item in collection)  // Explicit for complex types
```

---

## Expression-Bodied Members

### Use For
- **Accessors**: `public int Length => _values.Length;`
- **Indexers**: `public T this[int index] => _items[index];`
- **Lambdas**: `Func<int, int> double = x => x * 2;`

### Avoid For
- **Constructors**: Use block body
- **Destructors**: Use block body

```csharp
// Good
public string FullName => $"{FirstName} {LastName}";
public int Count => _items.Count;

// Avoid
public Person(string name) => Name = name;  // Too complex for primary constructor context
```

---

## LINQ Usage

### Method Syntax
Use for simple queries:
```csharp
var results = items.Where(x => x.IsActive).OrderBy(x => x.Name);
```

### Query Syntax
Use for complex queries or when it improves readability:
```csharp
var query = from order in orders
            join customer in customers on order.CustomerId equals customer.Id
            where order.Date > startDate
            select new { order.Id, customer.Name };
```

---

## String Handling

### Interpolation
Use for simple cases:
```csharp
var message = $"Hello, {customerName}!";
var displayName = $"{lastName}, {firstName}";
```

### StringBuilder
Use for concatenation in loops:
```csharp
var sb = new StringBuilder();
foreach (var item in items)
{
    sb.Append(item);
    sb.Append(", ");
}
```

### Raw String Literals
Use for multiline strings:
```csharp
var json = """
    {
        "name": "value",
        "items": [1, 2, 3]
    }
    """;
```

---

## Collection Initialization

### Collection Expressions (C# 12+)
```csharp
string[] vowels = ["a", "e", "i", "o", "u"];
List<int> numbers = [1, 2, 3];
Dictionary<string, int> ages = ["John": 30, "Jane": 25];
```

### Traditional
```csharp
var list = new List<int> { 1, 2, 3 };
var dict = new Dictionary<string, int>
{
    ["John"] = 30,
    ["Jane"] = 25
};
```

---

## Delegates

### Use Func<> and Action<>
```csharp
Action<string> logger = x => Console.WriteLine(x);
Func<int, int, int> add = (x, y) => x + y;
Func<string, bool> validator = s => !string.IsNullOrEmpty(s);
```

---

## Member Access

### Static Members
Call using class name:
```csharp
Console.WriteLine();
File.Open(path, mode);
Task.FromResult(value);
```

### Avoid this. Qualification
```csharp
public class Customer
{
    private string _name;
    
    public void SetName(string name)
    {
        _name = name;  // Not this._name
    }
}
```

---

## Records

### Primary Constructor Parameters
Use PascalCase for parameters (becomes public properties):
```csharp
public record Person(string FirstName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}";
}
```

---

## File-Scoped Namespaces (C# 10+)

Use for cleaner file organization:
```csharp
namespace MyNamespace;

public class MyClass { }
```
