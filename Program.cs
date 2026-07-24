using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Api.Data;
using SafeVault.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configure In-Memory Database for EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SafeVaultDb"));

builder.Services.AddScoped<IFinancialRecordService, FinancialRecordService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Configure ASP.NET Core Identity with strict security policies
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password complexity rules to protect SafeVault credentials
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12; // Enforce strong minimum length
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings to mitigate brute-force attacks
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. Configure JWT Authentication
// Note: In production, store secrets in Azure Key Vault or Environment Variables!
var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "SafeVault_Super_Secret_Development_Key_2026!";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // Enforce HTTPS transport security
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SafeVaultApi",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SafeVaultUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Eliminate default 5-min grace period on token expiration
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// 5. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// SECURITY CRITICAL: Order matters! Authentication must always precede Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();