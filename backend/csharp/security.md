# C# Security Best Practices

Based on [OWASP](https://owasp.org/) guidelines and Microsoft [Security Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/).

---

## Never Expose Secrets

### Don't Hardcode Credentials
```csharp
// Bad
var apiKey = "sk-1234567890abcdef";
var password = "MySecretPassword!";

// Good - use configuration
var apiKey = configuration["ApiKey"];
var password = Environment.GetEnvironmentVariable("PASSWORD");

// Better - use secret manager (development) or key vault (production)
var apiKey = secretClient.GetSecret("ApiKey");
```

### Use Environment Variables
```csharp
// In production
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

// With Azure Key Vault
var keyVaultUrl = new Uri(Environment.GetEnvironmentVariable("KEY_VAULT_URL"));
var credential = new DefaultAzureCredential();
var client = new SecretClient(keyVaultUrl, credential);
```

---

## Input Validation

### Validate All Inputs
```csharp
public void ProcessOrder(string customerName, string email, int quantity)
{
    if (string.IsNullOrWhiteSpace(customerName))
        throw new ArgumentException("Customer name is required", nameof(customerName));
        
    if (!IsValidEmail(email))
        throw new ArgumentException("Invalid email format", nameof(email));
        
    if (quantity <= 0 || quantity > 1000)
        throw new ArgumentOutOfRangeException(nameof(quantity));
        
    // Process
}

private bool IsValidEmail(string email) => 
    !string.IsNullOrWhiteSpace(email) && email.Contains('@');
```

### Use Built-in Validation
```csharp
// DataAnnotations
public class OrderDto
{
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Range(1, 1000)]
    public int Quantity { get; set; }
}

// FluentValidation
public class OrderValidator : AbstractValidator<OrderDto>
{
    public OrderValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1000);
    }
}
```

---

## SQL Injection

### Use Parameterized Queries
```csharp
// Bad - SQL injection vulnerable
var sql = $"SELECT * FROM Users WHERE Name = '{username}'";
using var cmd = new SqlCommand(sql, connection);

// Good - parameterized
var sql = "SELECT * FROM Users WHERE Name = @username";
using var cmd = new SqlCommand(sql, connection);
cmd.Parameters.AddWithValue("@username", username);

// Good - Dapper
var user = connection.QueryFirstOrDefault<User>(
    "SELECT * FROM Users WHERE Name = @username", 
    new { username });
```

### Use ORMs
```csharp
// Entity Framework Core - automatically parameterized
var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Name == username);
```

---

## Cross-Site Scripting (XSS)

### Encode Output
```csharp
// In Razor views
@Html.Encode(userInput)
@Model.UserInput

// In ASP.NET Core (auto-encoded by default)
@userInput

// For explicit encoding
@Html.Encode(userInput)
System.Web.HttpUtility.HtmlEncode(userInput)
```

### Content Security Policy
```csharp
// In Startup.cs or Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline'");
    await next();
});
```

---

## Cross-Site Request Forgery (CSRF)

### Use Anti-Forgery Tokens
```csharp
// In Razor views
<form method="post">
    @Html.AntiForgeryToken()
    ...
</form>

// In API controllers (ASP.NET Core)
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Process(OrderDto order) { }

// For AJAX
var token = document.querySelector('input[name="__RequestVerificationToken"]').value;
```

---

## Authentication

### Use Standard Authentication
```csharp
// ASP.NET Core Identity
services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Or OAuth
services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Google:ClientId"];
        options.ClientSecret = configuration["Google:ClientSecret"];
    });
```

### Password Security
```csharp
// Use built-in password hasher
var hashedPassword = new PasswordHasher<User>().HashPassword(user, rawPassword);

// Verify
var result = new PasswordHasher<User>()
    .VerifyHashedPassword(user, user.PasswordHash, rawPassword);
```

---

## Authorization

### Use Role-Based Authorization
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Delete(int id) { }

// Or policy-based
services.AddAuthorization(options =>
{
    options.AddPolicy("CanDeleteOrders", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("permission", "delete_orders"));
});

[Authorize(Policy = "CanDeleteOrders")]
public async Task<IActionResult> Delete(int id) { }
```

### Principle of Least Privilege
```csharp
// Don't run with elevated permissions longer than necessary
using var impersonatedUser = WindowsIdentity.GetCurrent().Impersonate();
// Do work
impersonatedUser.Undo();
```

---

## Secure Coding Practices

### Dispose Sensitive Data
```csharp
// Use SecureString (with caution)
using var securePassword = new SecureString();
foreach (char c in password)
    securePassword.AppendChar(c);
securePassword.MakeReadOnly();

// Clear arrays with sensitive data
byte[] key = new byte[32];
try
{
    // Use key
}
finally
{
    Array.Clear(key, 0, key.Length);
}
```

### Avoid Information Disclosure
```csharp
// Bad - expose internal details
throw new Exception("Database connection failed: " + connectionString);

// Good - generic message
throw new Exception("An error occurred processing your request");

// Log details separately (with proper access controls)
_logger.LogError(ex, "Database error for connection {ConnectionId}", connectionId);
```

---

## Cryptography

### Use Strong Algorithms
```csharp
// Use built-in cryptographic methods
using var aes = Aes.Create();
aes.KeySize = 256;
aes.GenerateIV();

// Encrypt
using var encryptor = aes.CreateEncryptor();
var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

// Hash passwords - use BCrypt/Argon2 via libraries
var hashed = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
var valid = BCrypt.Net.BCrypt.VerifyPassword(password, hashed);
```

### Never Roll Your Own Crypto
```csharp
// Bad
string Encode(string input) => Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

// Good - use established libraries
using var sha256 = SHA256.Create();
var hash = sha256.ComputeHash(data);
```

---

## Secure Configuration

### Don't Store Secrets in Config Files
```csharp
// appsettings.json
{
    "ConnectionStrings": {
        // Don't do this!
        // "Default": "Server=.;Database=MyDb;User=sa;Password=Secret123!"
    }
}

// Use User Secrets in development
dotnet user-secrets set "ConnectionStrings:Default" "Server=..."

// Use Azure Key Vault in production
builder.Configuration.AddAzureKeyVault(...)
```

---

## Dependency Security

### Keep Dependencies Updated
```csharp
// Regularly run
dotnet list package --outdated
dotnet add package <PackageName>
```

### Use Vulnerability Scanning
```csharp
// Add to CI/CD
dotnet tool install --global dotnet-validate-dependencies
dotnet validate-dependencies
```
