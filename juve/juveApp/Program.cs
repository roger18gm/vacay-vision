using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using juveApp.Data;
using juveApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CommunityService>();

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

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
