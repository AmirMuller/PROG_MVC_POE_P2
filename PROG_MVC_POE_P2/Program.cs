using Microsoft.EntityFrameworkCore;
using PROG_MVC_POE_P2.Services;
using Microsoft.AspNetCore.DataProtection;
using PROG_MVC_POE_P2.Data.Models;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

//temp hr password set up
//var temp = PROG_MVC_POE_P2.Helpers.PasswordHelper.HashPassword("Password123!");
//Console.WriteLine("HASH: " + temp.hash);
//Console.WriteLine("SALT: " + temp.salt);

// data protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtectionKeys"));

// EF DbContext (use existing connection string in your DbContext or appsettings)
builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\MSSQLLocalDB;Database=dbClaims;Trusted_Connection=True;"));

// Add controllers with views
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

// PDF service
builder.Services.AddScoped<IPdfService, PdfService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
