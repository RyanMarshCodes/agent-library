# C# Code Analysis Rules

Based on Microsoft [Code Analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/) and [Code Style Rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/).

---

## Severity Levels

### Required (Treat as Errors)
| ID | Rule | Description |
|----|------|-------------|
| CA1062 | Validate arguments of public methods | Null-check public method arguments |
| CA2007 | Do not directly await a Task | Use `ConfigureAwait` or add analyzer package |
| CA5392 | Use DefaultParameterValue for P/Invokes | Security risk |

### Recommended (Warnings)
| ID | Rule | Description |
|----|------|-------------|
| CA1000 | Do not declare static members on generic types | Use interface instead |
| CA1014 | Mark assemblies with CLSCompliantAttribute | Ensure cross-language compatibility |
| CA1065 | Do not raise exceptions in unexpected locations | Don't throw from properties/indexers unnecessarily |
| CA1303 | Do not pass literals as localized parameters | Use named constants |
| CA1508 | Avoid dead conditional code | Remove unreachable code |
| CA2002 | Do not lock on objects with weak identity | Use private lock objects |
| CA2100 | Review SQL queries for security risks | Use parameterized queries |
| CA2200 | Rethrow to preserve stack details | Use `throw;` not `throw ex;` |
| CA2213 | Disposable fields should be disposed | Proper disposal of fields |
| CA2254 | Template should be static | Logging templates should not be dynamic |

---

## Naming Rules

### Required
| ID | Rule | Description |
|----|------|-------------|
| CA1707 | Identifiers should not contain underscores | Avoid `_` in names |
| CA1708 | Identifiers should differ by more than case | `name` and `Name` conflict in case-insensitive languages |
| CA1710 | Identifiers should have correct suffix | Repositories → `IRepository<T>` |
| CA1711 | Identifiers should not have incorrect suffix | Don't add "Attribute" manually |
| CA1712 | Do not prefix enum values with type name | `OrderStatus.Active`, not `OrderStatus.OrderStatusActive` |
| CA1713 | Events should not have before/after prefix | Use present/past tense: `Closing`/`Closed` |
| CA1714 | Flags enums should have plural names | `[Flags] public enum FileModes` |
| CA1715 | Identifiers should have correct prefix | Interfaces start with `I` |
| CA1716 | Identifiers should not match keywords | Don't use `class`, `event`, etc. |
| CA1720 | Identifiers should not contain type names | `id` not `customerId` for parameter |
| CA1721 | Property names should not match get methods | `GetName()` vs `Name` property conflict |
| CA1724 | Type Names Should Not Match Namespaces | Don't shadow `System` namespace |
| CA1725 | Parameter names should match base declaration | Consistent override parameter names |

### Recommended
| ID | Rule | Description |
|----|------|-------------|
| CA1713 | Events should not have before or after prefix | Use `Opening`/`Closing` not `BeforeOpen`/`AfterOpen` |
| CA1727 | Use PascalCase for named placeholders | Logging: `"User {UserId}"` not `"User {userId}"` |

---

## Design Rules

### Required
| ID | Rule | Description |
|----|------|-------------|
| CA1002 | Do not expose generic lists | Use `IReadOnlyList<T>` or `IEnumerable<T>` |
| CA1008 | Enums should have zero value | Define `None = 0` |
| CA1012 | Abstract classes should not have public constructors | Use protected constructors |
| CA1019 | Define accessors for attribute arguments | Make attribute properties public |
| CA1027 | Mark enums with FlagsAttribute | For combinable enums |
| CA1034 | Nested types should not be visible | Don't expose nested types publicly |
| CA1040 | Avoid empty interfaces | Use markers or base classes |
| CA1050 | Declare types in namespaces | Required for proper organization |
| CA1051 | Do not declare visible instance fields | Use properties |
| CA1052 | Static holder types should be sealed | Make static classes sealed |
| CA1054 | URI parameters should not be strings | Use `Uri` type |
| CA1056 | URI properties should not be strings | Use `Uri` type |
| CA1060 | Use Marshal.GetLastWin32Error | P/Invoke error handling |
| CA1063 | Implement IDisposable correctly | Proper disposal pattern |
| CA1064 | Exceptions should be public | Custom exceptions should be public |

### Recommended
| ID | Rule | Description |
|----|------|-------------|
| CA1003 | Use generic event handler instances | `EventHandler<T>` |
| CA1018 | Mark attributes with AttributeUsageAttribute | Specify valid targets |
| CA1021 | Avoid out parameters | Consider returning tuple or type |
| CA1028 | Enum Storage should be Int32 | Use int for enums |
| CA1031 | Catch Exception types when required | Don't catch all unless necessary |
| CA1036 | Override methods on comparable types | Implement `IComparable<T>` |
| CA1045 | Do not pass types by reference | Consider copy semantics |
| CA1051 | Do not declare visible instance fields | Use properties instead |
| CA1061 | Do not hide base class methods | Don't hide `Equals` without `override` |

---

## Performance Rules

| ID | Rule | Description |
|----|------|-------------|
| CA1802 | Use literals where appropriate | Compile-time constants |
| CA1805 | Do not initialize unnecessarily | Default initialization |
| CA1806 | Do not ignore method results | Don't discard return values |
| CA1810 | Initialize reference type static fields inline | Performance |
| CA1812 | Avoid uninstantiated internal classes | Remove unused classes |
| CA1813 | Avoid unsealed attributes | Allow inheritance |
| CA1815 | Override equals and operator equals on value types | Value type equality |
| CA1819 | Properties should not return arrays | Consider collection |
| CA1820 | Test for empty strings using string length | Performance |
| CA1821 | Remove empty Finalizers | Performance |
| CA1822 | Mark members as static | Non-instance members |
| CA1823 | Avoid unused private fields | Remove dead code |
| CA1824 | Mark assemblies with NeutralResourcesLanguageAttribute | Localization |

---

## Security Rules

### Required
| ID | Rule | Description |
|----|------|-------------|
| CA2100 | Review SQL queries for security risks | Use parameterized queries |
| CA2101 | Specify marshaling for P/Invoke string arguments | Security |
| CA2119 | Seal methods that satisfy private interfaces | Security |
| CA2120 | Secure serialization constructors | Security |
| CA2121 | Static constructors should be private | Security |
| CA5122 | Do not make assemblies callable from unmanaged code | P/Invoke security |

---

## Maintainability Rules

| ID | Rule | Description |
|----|------|-------------|
| CA1500 | Variable names should not match field names | Avoid confusion |
| CA1501 | Avoid excessive inheritance | Max 5 levels |
| CA1502 | Avoid excessive complexity | Method cyclomatic complexity |
| CA1503 | Ensure null check is not redundant | Unnecessary null checks |
| CA1504 | Misleading variable names | `_` prefix for fields |
| CA1505 | Avoid unmaintainable code | High cyclomatic complexity |
| CA1506 | Avoid excessive class coupling | Too many dependencies |
| CA1507 | Use nameof in attribute | String literals for member names |
| CA1508 | Avoid dead conditional code | Unreachable code |
| CA1509 | Invalid entry in code metrics configuration | Config file errors |

---

## Portability Rules

| ID | Rule | Description |
|----|------|-------------|
| CA1301 | Avoid duplicate keyboard shortcuts | UI accessibility |
| CA1303 | Do not pass literals as localized parameters | Use string constants |
| CA1304 | Specify CultureInfo | Culture-aware operations |
| CA1305 | Specify IFormatProvider | String formatting |
| CA1307 | Specify StringComparison | String comparisons |
| CA1308 | Normalize uppercase strings | Security |
| CA1309 | Use ordinal StringComparison | Performance |
| CA1310 | Specify StringComparison for correctness | String operations |

---

## Usage Rules

| ID | Rule | Description |
|----|------|-------------|
| CA1800 | Do not cast unnecessarily | Avoid unnecessary casts |
| CA1801 | Review unused parameters | Remove or use params |
| CA1802 | Use literals where appropriate | Static readonly vs const |
| CA1804 | Remove unused locals | Clean code |
| CA1805 | Do not initialize unnecessarily | Default values |
| CA1806 | Do not ignore method results | Don't discard returns |
| CA1808 | Do not use Count() or LongCount() | Use Any() when possible |
| CA1809 | Avoid excessive locals | Performance |
| CA1811 | Avoid uncalled private code | Remove dead code |
| CA1812 | Avoid uninstantiated internal classes | Remove unused |
| CA1813 | Avoid unsealed attributes | Allow extension |
| CA1814 | Prefer jagged arrays over multidimensional | Performance |
| CA1815 | Override equals on value types | Value equality |
| CA1816 | Dispose methods should call SuppressFinalize | GC.SuppressFinalize |
| CA1818 | Do not pass readonly span/take-home arrays | Arrays passed to methods |
| CA1819 | Properties should not return arrays | Return collection |
| CA1820 | Test for empty strings using string length | Performance |
| CA1821 | Remove empty finalizers | Performance |
| CA1822 | Mark members as static | Performance |
| CA1823 | Avoid unused private fields | Remove dead fields |
| CA1824 | Mark with NeutralResourcesLanguageAttribute | Localization |

---

## Enabling Rules in Project

Add to your `.csproj`:

```xml
<PropertyGroup>
  <AnalysisMode>Recommended</AnalysisMode>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <NoWarn>CAxxxx;IDExxxx</NoWarn>  <!-- List suppressed rules -->
</PropertyGroup>
```

Or use `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.CA1062.severity = error
dotnet_diagnostic.CA1707.severity = warning
```
