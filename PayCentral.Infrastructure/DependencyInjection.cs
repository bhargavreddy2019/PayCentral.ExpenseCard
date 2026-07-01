using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Fraud;
using PayCentral.Infrastructure.Persistence;
using PayCentral.Infrastructure.Services;
using System.Text;

namespace PayCentral.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext, AppDbContext>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Fraud rules
        services.AddScoped<IFraudRule, LargeSpendRule>();
        services.AddScoped<IFraudRule, InternationalTransactionRule>();
        services.AddScoped<IFraudRule, RapidPurchaseRule>();
        services.AddScoped<IFraudRule, MultipleMerchantCategoriesRule>();
        services.AddScoped<IFraudRule, FailedTransactionsRule>();

        // Fraud services
        services.AddScoped<IFraudService, FraudService>();
        services.AddScoped<IFraudHubService, FraudHubService>();

        services.AddScoped<ICsvExportService, CsvExportService>();

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Administrator"))
            .AddPolicy("CardholderOnly", policy =>
                policy.RequireRole("Cardholder"))
            .AddPolicy("AdminOrCardholder", policy =>
                policy.RequireRole("Administrator", "Cardholder"));

        return services;
    }
}