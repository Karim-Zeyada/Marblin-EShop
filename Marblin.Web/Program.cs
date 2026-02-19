using Marblin.Infrastructure.Data;
using Marblin.Web.Services;
using Marblin.Infrastructure.Services;
using Marblin.Core.Interfaces;
using Marblin.Application.Interfaces;
using Marblin.Application.Services;
using Marblin.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure login/logout redirect paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Session and Cart Services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// =============================================================================
// INFRASTRUCTURE LAYER - Data Access, Repositories, External Services
// =============================================================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IFileService, LocalFileService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// =============================================================================
// APPLICATION LAYER - Use Cases, Application Services
// =============================================================================
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderFactory, Marblin.Application.Services.OrderFactory>();

// =============================================================================
// WEB LAYER - Presentation Services (Storage Adapters)
// =============================================================================
builder.Services.AddScoped<ICartStorage, SessionCartStorage>();
builder.Services.AddScoped<ICartService, Marblin.Application.Services.CartService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// =============================================================================
// RATE LIMITING - Protection against brute force attacks
// =============================================================================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("TrackingPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // Use custom Global Exception Handling Middleware
    app.UseMiddleware<Marblin.Web.Middleware.GlobalExceptionHandlingMiddleware>();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure Global Metrics (Currency)
var defaultCulture = new System.Globalization.CultureInfo("en-US");
defaultCulture.NumberFormat.CurrencySymbol = "EGP ";
defaultCulture.NumberFormat.CurrencyPositivePattern = 0; // $n
defaultCulture.NumberFormat.CurrencyNegativePattern = 0; // ($n)

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(defaultCulture),
    SupportedCultures = new List<System.Globalization.CultureInfo> { defaultCulture },
    SupportedUICultures = new List<System.Globalization.CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await Marblin.Infrastructure.Data.DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
