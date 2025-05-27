using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.Cart;
using APIClothesEcommerceShop.Repositories.Cart_DetailCombo;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.CategoryDetails;
using APIClothesEcommerceShop.Repositories.Customer;
using APIClothesEcommerceShop.Repositories.ImageProduct;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Repositories.OrderComboDetails;
using APIClothesEcommerceShop.Repositories.OrderDetails;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.ProductDetails;
using APIClothesEcommerceShop.Services;
using APIClothesEcommerceShop.Repositories.Staff;
using APIClothesEcommerceShop.Repositories.Statistics;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;
using VNPAY.NET;
using APIClothesEcommerceShop.Repositories.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EcommerceShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceShopConnect"));
});

// Add services to the container.

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Vô hi?u hóa validate t? ??ng ?? tránh thông báo l?i m?c ??nh
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

    #region Format thÃªm comment lÃªn mÃ´i action
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    #endregion

    var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                };

    c.AddSecurityRequirement(securityRequirement);

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
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IVnpay, Vnpay>();
builder.Services.AddScoped<IOrderDetails, OrderDetails>();
builder.Services.AddScoped<IOrderComboDetails, OrderComboDetails>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICart_DetailComboRepository, Cart_DetailComboRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

SeedDatabaes();

app.MapControllers();

app.Run();

#region Func táº¡o CConstantsL 
void SeedDatabaes()
{
    using (var seedScope = app.Services.CreateScope())
    {
        var dbInitializer = seedScope.ServiceProvider.GetRequiredService<IDbInitializer>();
        try
        {
            dbInitializer.InitializeDb();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
#endregion