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
using APIClothesEcommerceShop.Repositories.HashPassword;
using APIClothesEcommerceShop.Repositories.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using APIClothesEcommerceShop.Repositories.Account;
using Humanizer.Configuration;
using VNPAY.NET;
using APIClothesEcommerceShop.Repositories.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

/* 
Cấu hình kết nối đến database
EcommerceShopConnect_TD - Data Source=NGUYENTHANHDATP
EcommerceShopConnect_PM - Data Source=DESKTOP..PHAMHAU
EcommerceShopConnect_Dot - Data Source=.;
 */
builder.Services.AddDbContext<EcommerceShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceShopConnect_Dot"));
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

    #region Format thêm comment lên môi action
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
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductDetailsRepository, ProductDetailsRepository>();
builder.Services.AddScoped<ICategoryDetailsRepository, CategoryDetailsRepository>();
builder.Services.AddScoped<IImageProductRepository, ImageProductRepository>();
builder.Services.AddScoped<ProductService>();
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

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
var SecretKey = builder.Configuration["JWT:SecretKey"];
var SecretKeyBytes = Encoding.UTF8.GetBytes(SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(options =>
{
    var googleAuth = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuth["ClientId"];
    options.ClientSecret = googleAuth["ClientSecret"];
});
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
SeedDatabaes();

app.MapControllers();

app.Run();

#region Func tạo CConstantsL 
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
