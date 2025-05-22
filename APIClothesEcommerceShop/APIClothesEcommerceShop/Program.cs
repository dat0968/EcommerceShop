using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.CategoryDetails;
using APIClothesEcommerceShop.Repositories.Customer;
using APIClothesEcommerceShop.Repositories.ImageProduct;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.ProductDetails;
using APIClothesEcommerceShop.Repositories.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // V� hi?u h�a validate t? ??ng ?? tr�nh th�ng b�o l?i m?c ??nh
    options.ModelValidatorProviders.Clear();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyProject", Version = "v1.0.0" });

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securitySchema);

    var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                };

    c.AddSecurityRequirement(securityRequirement);

});
builder.Services.AddDbContext<EcommerceShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceShopConnect"));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", ops =>
    {
        ops.AllowAnyHeader();
        ops.AllowAnyMethod();
        ops.AllowAnyOrigin();
        ops.SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductDetailsRepository, ProductDetailsRepository>();
builder.Services.AddScoped<ICategoryDetailsRepository, CategoryDetailsRepository>();
builder.Services.AddScoped<IImageProductRepository, ImageProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();
