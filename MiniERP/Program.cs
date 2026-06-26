using FluentValidation;
using MiniERP.Filters;
using MiniERP.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<NormalizeFilter>();
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký CSDL, Repositories và Services qua Extension Methods
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Đăng ký Validators từ Application Assembly
builder.Services.AddValidatorsFromAssembly(typeof(MiniERP.Application.Validators.Customers.CreateCustomerRequestValidator).Assembly);

var app = builder.Build();

// Sử dụng exception middleware qua Extension Method
app.UseGlobalExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
