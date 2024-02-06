using Microsoft.AspNetCore.Builder;
using AppBlog.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var stringConnectDb = builder.Configuration.GetConnectionString("dbBLog");
builder.Services.AddDbContext<BlogContext>(options=> options.UseSqlServer(stringConnectDb));
builder.Services.AddSession();
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("CookieAuthentication")
    .AddCookie("CookieAuthentication", config=>
    {
        config.Cookie.Name = "UserLoginCookie";
        config.ExpireTimeSpan = TimeSpan.FromDays(1);
        config.LoginPath = "/dang-nhap.html";
        config.LogoutPath = "/dang-xuat.html";
        config.AccessDeniedPath = "/not-found.html";
    });
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Expiration = TimeSpan.FromDays(150);
    options.ExpireTimeSpan = TimeSpan.FromDays(150);
});

var app = builder.Build();
app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();



app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
