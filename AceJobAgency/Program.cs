using AceJobAgency.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Database Context using AuthDbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString")));

// Add Identity and configure to use ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Add Razor Pages
builder.Services.AddRazorPages();

// Enable session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Set session timeout to 20 minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true; // Ensure session cookies are always sent
});

// Configure Identity Cookie Settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Set session timeout
    options.SlidingExpiration = false; // Ensure session expires at exact time
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401; // API requests return 401
        }
        else
        {
            context.Response.Redirect("/Login"); // Always redirect UI pages to login
        }
        return Task.CompletedTask;
    };
});

builder.Services.AddHttpClient<ReCaptchaService>();

// Configure Authorization Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBelongToHRDepartment",
        policy => policy.RequireClaim("Department", "HR"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session before authentication & authorization
app.UseSession(); // <---- Add this before authentication

app.UseAuthentication();  // Ensure authentication middleware is added
app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/api/session-check", async (HttpContext context) =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = 401; // Unauthorized
    }
    else
    {
        context.Response.StatusCode = 200; // Session still active
    }
});

app.Run(); // Must be last