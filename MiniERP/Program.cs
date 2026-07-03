using FluentValidation;
using MiniERP.Filters;
using MiniERP.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire;
using MiniERP.Application.Interfaces.Services;
using MiniERP.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<NormalizeFilter>();
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<CheckUserStatusFilter>();
});
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger với JWT Security Definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Mini ERP API", Version = "v1" });
    
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };
    
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Đăng ký CSDL, Repositories và Services qua Extension Methods
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddBackgroundJobs(builder.Configuration);

// Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("RequireReadAccess", policy => policy.RequireRole("ADMIN", "MANAGER", "STAFF", "ACCOUNTANT"));
    options.AddPolicy("RequireWriteAccess", policy => policy.RequireRole("ADMIN", "MANAGER", "STAFF"));
    options.AddPolicy("RequireOrderRead", policy => policy.RequireRole("ADMIN", "MANAGER", "STAFF"));
    options.AddPolicy("RequireOrderCreate", policy => policy.RequireRole("ADMIN", "STAFF"));
    options.AddPolicy("RequireOrderApprove", policy => policy.RequireRole("ADMIN", "MANAGER"));
    options.AddPolicy("RequireInventoryWrite", policy => policy.RequireRole("ADMIN", "STAFF"));
    options.AddPolicy("RequirePaymentRead", policy => policy.RequireRole("ADMIN", "ACCOUNTANT", "STAFF"));
    options.AddPolicy("RequirePaymentWrite", policy => policy.RequireRole("ADMIN", "ACCOUNTANT"));
    options.AddPolicy("RequireReportAccess", policy => policy.RequireRole("ADMIN", "MANAGER", "ACCOUNTANT"));
});
// Đăng ký Validators từ Application Assembly
builder.Services.AddValidatorsFromAssembly(typeof(MiniERP.Application.Validators.Customers.CreateCustomerRequestValidator).Assembly);

var app = builder.Build();

// Sử dụng exception middleware qua Extension Method
app.UseGlobalExceptionHandling();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

// Đăng ký Daily Summary Job chạy hằng ngày lúc 00:05 (giờ Việt Nam)
TimeZoneInfo vietnamTimeZone;
try
{
    vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
}
catch (TimeZoneNotFoundException)
{
    vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.SeedAsync(context).GetAwaiter().GetResult();

    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<IDailySummaryJob>(
        "daily-summary-job",
        job => job.ExecuteAsync(null),
        "5 0 * * *", // Chạy lúc 00:05 hằng ngày theo giờ Việt Nam
        new RecurringJobOptions { TimeZone = vietnamTimeZone }
    );
}

app.MapControllers();

app.Run();
