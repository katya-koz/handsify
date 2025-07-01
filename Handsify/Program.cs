using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Handsify;
using System.Text;
using SALLY_API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Handsify.utils.APIClient>();

// Add services to the container.

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JwtTokenManager>();

// Add distributed memory cache for session storage
builder.Services.AddDistributedMemoryCache();  // Required for in-memory session storage

// Enable session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set the session timeout duration
    options.Cookie.HttpOnly = true;  // Makes the cookie HttpOnly (security best practice)
    options.Cookie.IsEssential = true;  // Makes the cookie essential (required for GDPR compliance)
});


////Console.WriteLine((config.GetValue<string>("JwtConfig:Key"));


//session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);  // Set the session timeout duration
    options.Cookie.HttpOnly = true;  // Makes the cookie HttpOnly (security best practice)
    options.Cookie.IsEssential = true;  // Makes the cookie essential (required for GDPR compliance)
});


var config = builder.Configuration;

var dotenv = config["EnvironmentSettings:EnvFilePath"];
//Console.WriteLine((dotenv);
DotEnv.Load(dotenv);


var jwtKey = Environment.GetEnvironmentVariable("JwtConfigKey");
var issuer = Environment.GetEnvironmentVariable("Issuer");
var audience = Environment.GetEnvironmentVariable("Audience");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(jwtOptions =>
    {
        var key = jwtKey;
        var keyBytes = Encoding.ASCII.GetBytes(key);

        //jwtOptions.SaveToken = true;
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            RequireExpirationTime = true,
        };

        jwtOptions.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            // This event is triggered when a JWT fails validation (like when expired or invalid)
            OnChallenge = context =>
            {
                context.HandleResponse(); // Prevent the default 401 response  
                context.HttpContext.Response.Redirect("/login-redirect");
                return Task.CompletedTask;
                //return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new {result = "redirect",url = "/login"  })); // send this back to javascript to handle the page redirect

            }
        };
    });
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login"; // Redirect to the login page if not authenticated
    // options.AccessDeniedPath = "/AccessDenied"; // Optional: Set an access denied page
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Add Authentication services

// Configure Cookie Authentication (if needed for non-JWT parts of your app)

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "Home",
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.MapControllerRoute(
    name: "view-mode",
    pattern: "{controller=ViewMode}/{action=ViewModeIndex}/{id?}");

app.MapControllerRoute(
    name: "operation-mode",
    pattern: "{controller=OperationMode}/{action=OperationModeIndex}/{id?}");

app.MapControllerRoute(
    name: "edit-mode",
    pattern: "{controller=EditMode}/{action=EditModeIndex}/{id?}");

app.Run();
