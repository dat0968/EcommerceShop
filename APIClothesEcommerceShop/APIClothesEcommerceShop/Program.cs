using System.Reflection;
using System.Text;
using APIClothesEcommerceShop.Controllers;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.Account;
using APIClothesEcommerceShop.Repositories.Address;
using APIClothesEcommerceShop.Repositories.Cart;
using APIClothesEcommerceShop.Repositories.Cart_DetailCombo;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.CategoryDetails;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories.Combos;
using APIClothesEcommerceShop.Repositories.Coupon;
using APIClothesEcommerceShop.Repositories.Customer;
using APIClothesEcommerceShop.Repositories.DbInitializer;
using APIClothesEcommerceShop.Repositories.DetailCombo;
using APIClothesEcommerceShop.Repositories.FavoriteProduct;
using APIClothesEcommerceShop.Repositories.HashPassword;
using APIClothesEcommerceShop.Repositories.Home;
using APIClothesEcommerceShop.Repositories.ImageProduct;
using APIClothesEcommerceShop.Repositories.Macoupon;
using APIClothesEcommerceShop.Repositories.Order;
using APIClothesEcommerceShop.Repositories.OrderComboDetails;
using APIClothesEcommerceShop.Repositories.OrderDetails;
using APIClothesEcommerceShop.Repositories.Product;
using APIClothesEcommerceShop.Repositories.ProductDetails;
using APIClothesEcommerceShop.Repositories.Staff;
using APIClothesEcommerceShop.Repositories.Statistics;
using APIClothesEcommerceShop.Repositories.Token;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using APIClothesEcommerceShop.Repositories.ViewHistory;
using APIClothesEcommerceShop.Services;
using APIClothesEcommerceShop.Services.EmailService;
using APIClothesEcommerceShop.Services.EmailService.GoogleSenderService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using VNPAY.NET;
using APIClothesEcommerceShop.Repositories.Contact;
var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// Configure Kestrel to support both HTTP and HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7218); // HTTP for mobile
    options.ListenAnyIP(7217, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS for web
    });
});
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
/*
Cấu hình kết nối đến database
EcommerceShopConnect_TD - Data Source=NGUYENTHANHDATP
EcommerceShopConnect_PM - Data Source=DESKTOP..PHAMHAU
EcommerceShopConnect_Dot - Data Source=.;
 */
builder.Services.AddDbContext<EcommerceShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceShopConnect_TD"));
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.ModelValidatorProviders.Clear();
});

// Configure Swagger/OpenAPI
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

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    };

    c.AddSecurityRequirement(securityRequirement);
    #endregion
});

// Configure CORS for web and mobile (config-driven)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        }
        else
        {
            policy.AllowAnyOrigin()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        }
        // .AllowCredentials() // enable if you need cookies/credentials
    });
});

// Dependency Injection
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductDetailsRepository, ProductDetailsRepository>();
builder.Services.AddScoped<ICategoryDetailsRepository, CategoryDetailsRepository>();
builder.Services.AddScoped<IImageProductRepository, ImageProductRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<MLRecommendationSystem>();
builder.Services.AddScoped<ComboService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IVnpay, Vnpay>();
builder.Services.AddScoped<IOrderDetails, OrderDetails>();
builder.Services.AddScoped<IOrderComboDetails, OrderComboDetails>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IFavoriteProduct, FavoriteProduct>();
builder.Services.AddScoped<ICart_DetailComboRepository, Cart_DetailComboRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IMaCouponRepository, MaCouponRepository>();
builder.Services.AddScoped<IComboRepository, ComboRepository>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IDetailCombo, DetailCombo>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IViewHistoryRepository, ViewHistoryRepository>();
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();
builder.Services.AddScoped<APIClothesEcommerceShop.Services.CloudinaryService.ICloudinaryService, APIClothesEcommerceShop.Services.CloudinaryService.CloudinaryService>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
// Email Service
builder.Services.AddScoped<GoogleSenderService>();
var emailSettings = builder.Configuration.GetSection("GoogleEmailSetting");
builder.Services.Configure<GoogleEmailSetting>(emailSettings);

// Redis
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration["Redis:Configuration"];
//    options.InstanceName = builder.Configuration["Redis:InstanceName"];
//});


#region JWT Authentication
var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
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
    IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ClockSkew = TimeSpan.Zero
    };
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(options =>
{
    var googleAuth = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuth["ClientId"] ?? string.Empty;
    options.ClientSecret = googleAuth["ClientSecret"] ?? string.Empty;
});
#endregion

// Enable detailed logging for mobile debugging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("MyPolicy");
app.UseRouting();


// Middleware for mobile headers and logging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var userAgent = context.Request.Headers["User-Agent"].ToString();
    var origin = context.Request.Headers["Origin"].ToString();

    // Log mobile requests
    if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("Capacitor"))
    {
        logger.LogInformation($"📱 Mobile Request: {context.Request.Method} {context.Request.Path}");
        logger.LogInformation($"User-Agent: {userAgent}");
        logger.LogInformation($"Origin: {origin}");
    }

    // Ensure CORS headers are present only if not already added by UseCors
    if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
    {
        var allowed = new HashSet<string>(allowedOrigins, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(origin) && (allowed.Count == 0 || allowed.Contains(origin)))
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        }
        else
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        }
    }

    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, Origin, X-Requested-With");
    context.Response.Headers.Add("Access-Control-Max-Age", "86400");

    // Handle preflight requests
    if (context.Request.Method == "OPTIONS")
    {
        logger.LogInformation($"✈️ Preflight Request: {context.Request.Path} from {origin}");
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("");
        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();

// Health check and test endpoints for mobile
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        server = Environment.MachineName,
        environment = app.Environment.EnvironmentName,
        message = "API đang hoạt động bình thường",
        mobileSupport = true,
        endpoints = new
        {
            health = "/api/health",
            test = "/api/test",
            login = "/api/Account/LoginCustomer"
        }
    });
});

app.MapGet("/api/test", () =>
{
    return Results.Ok(new
    {
        message = "API test thành công!",
        timestamp = DateTime.UtcNow,
        supportMobile = true,
        server = Environment.MachineName,
        cors = "enabled"
    });
});

app.MapControllers();

// Server startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 =================================");
logger.LogInformation("🚀 DUAL PROTOCOL SERVER STARTUP");
logger.LogInformation("🚀 =================================");
logger.LogInformation("🌐 Server listening on:");
logger.LogInformation("   📡 HTTP:  http://0.0.0.0:7218");
logger.LogInformation("   🔒 HTTPS: https://0.0.0.0:7217");
logger.LogInformation("🛠️  HTTP Access (Port 7218):");
logger.LogInformation("   📚 Swagger: http://localhost:7218/swagger/index.html");
logger.LogInformation("   🏥 Health:  http://localhost:7218/api/health");
logger.LogInformation("   🧪 Test:    http://localhost:7218/api/test");
logger.LogInformation("   🔐 Login:   http://localhost:7218/api/Account/LoginCustomer");
logger.LogInformation("🔒 HTTPS Access (Port 7217):");
logger.LogInformation("   📚 Swagger: https://localhost:7217/swagger/index.html");
logger.LogInformation("   🏥 Health:  https://localhost:7217/api/health");
logger.LogInformation("   🧪 Test:    https://localhost:7217/api/test");
logger.LogInformation("   🔐 Login:   https://localhost:7217/api/Account/LoginCustomer");
logger.LogInformation("📱 Mobile Access (HTTP):");
logger.LogInformation("   📍 All APIs: http://192.168.1.150:7218/api/*");
logger.LogInformation("⚙️  CORS: Enabled for all origins");
logger.LogInformation("🔀 Auto-Redirect: DISABLED (Both ports work independently)");
logger.LogInformation("📱 Mobile Support: ENABLED (HTTP Port 7218)");
logger.LogInformation("🌐 Web Support: ENABLED (Both HTTPS:7217 + HTTP:7218)");
logger.LogInformation("🌍 Environment: " + app.Environment.EnvironmentName);
logger.LogInformation("💡 Note: Same APIs available on both ports");
logger.LogInformation("🚀 =================================");

app.Run();

#region Func tạo CConstantsL 
void SeedDatabase()
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