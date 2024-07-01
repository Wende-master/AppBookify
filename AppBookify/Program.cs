using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using AppBookify.Helpers;
using AppBookify.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using AppCoreBookify.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


builder.Services.AddTransient<HelperPathProvider>();
builder.Services.AddTransient<HelperTools>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
    config =>
    {
        config.AccessDeniedPath = "/Managed/ErrorAcceso";
        config.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        config.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    context.Response.Redirect("/auth/Login");
                }
                return Task.CompletedTask;
            }
        };
    }
    );

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador",
        policy => policy.RequireClaim("ADMIN"));

});

builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false)
    .AddSessionStateTempDataProvider();


builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient =
builder.Services.BuildServiceProvider().GetService<SecretClient>();


////CACHE REDIS
//var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
//var credential = new DefaultAzureCredential();
//secretClient = new SecretClient(new Uri(keyVaultUri), credential);

//builder.Services.AddStackExchangeRedisCache(async options =>
//{
//    KeyVaultSecret secretCache = await secretClient.GetSecretAsync("CacheRedisBookify");
//    options.Configuration = secretCache.Value;
//});

//builder.Services.AddTransient<ServiceCacheRedis>();


//STORAGE
KeyVaultSecret secretStorage =
    await secretClient.GetSecretAsync("StorageAccount");

//CONTENEDORES BLOBS
string secretBlobKey = secretStorage.Value;

BlobServiceClient blobServiceClient =
    new BlobServiceClient(secretBlobKey);
builder.Services.AddTransient<BlobServiceClient>(x => blobServiceClient);
builder.Services.AddTransient<ServiceStorageBlobs>();


builder.Services.AddTransient<ServiceMail>();

builder.Services.AddTransient<ServiceBookify>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});


var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");

//    app.UseHsts();
//}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}"
        );
});

app.Run();

