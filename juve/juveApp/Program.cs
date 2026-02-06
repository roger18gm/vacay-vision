using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using juveApp.Data;
using juveApp.Services;
using juveApp.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog to read from appsettings with environment
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

try
{
    Log.Information("Starting VacayVision application");

    // Add services to the container
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    // Add HTTP request logging with detailed output
    builder.Services.AddW3CLogging(logging =>
    {
        logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.W3CLoggingFields.All;
        logging.FileSizeLimit = 5 * 1024 * 1024;
        logging.RetainedFileCountLimit = 2;
        logging.FlushInterval = TimeSpan.FromSeconds(2);
    });

    // Register services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<CommunityService>();
    builder.Services.AddScoped<VacationService>();
    builder.Services.AddScoped<CommentService>();
    builder.Services.AddScoped<DashboardService>();
    builder.Services.AddScoped<UserService>();

    // Configure session
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.Name = ".VacayVision.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Configure authentication with cookies
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/auth/login";
            options.LogoutPath = "/auth/logout";
            options.AccessDeniedPath = "/auth/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.Cookie.Name = ".VacayVision.Auth";
            options.Cookie.HttpOnly = true;
            options.SlidingExpiration = true;
        });

    var app = builder.Build();

    // Clear old user data with invalid hashes on startup (development only)
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // await DbInitializer.ClearUserDataAsync(context);
        }
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error/500");
        app.UseHsts();
    }

    // Status code pages for 404, etc.
    app.UseStatusCodePagesWithReExecute("/error/{0}");

    // Security headers
    app.UseSecurityHeaders();

    // W3C logging
    app.UseW3CLogging();

    // Custom request logging
    app.Use(async (context, next) =>
    {
        var start = DateTime.UtcNow;
        await next();
        var duration = DateTime.UtcNow - start;
        Log.Information("HTTP {Method} {Path} responded {StatusCode} in {Duration}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            duration.TotalMilliseconds);
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    // Middleware order matters! Authentication must come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSession();

    app.MapControllers();
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
