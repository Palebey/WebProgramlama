using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý
builder.Services.AddDbContext<SporSalonuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SporSalonuConnection")));

// 2. Identity Servislerinin Eklenmesi (Giriþ/Çýkýþ ve Rol Yönetimi)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Ödev için basit þifre kurallarý
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SporSalonuDbContext>()
.AddDefaultTokenProviders();

// 3. Giriþ Yapýlmamýþsa Yönlendirme Ayarlarý
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 4. Razor Pages Ayarlarý
builder.Services.AddRazorPages(options =>
{
    // Admin klasörüne sadece giriþ yapmýþ olanlar eriþebilir.
    options.Conventions.AuthorizeFolder("/Admin");
});

// --- YENÝ EKLENEN KISIM: Controller Servisi ---
// REST API (RandevuApiController) kullanabilmek için bu servis þarttýr.
builder.Services.AddControllers();
// ---------------------------------------------
builder.Services.AddHttpClient<web.Services.GeminiService>();

var app = builder.Build();

// --- ÖDEV GEREKSÝNÝMÝ: OTOMATÝK ADMÝN VE ROL OLUÞTURMA ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Uye" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "b221210003@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(newAdmin, "sau");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Roller veya Admin oluþturulurken hata: " + ex.Message);
    }
}
// -----------------------------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. ÖNEMLÝ: Authentication Authorization'dan ÖNCE gelmelidir.
app.UseAuthentication();
app.UseAuthorization();

// Controller ve Razor Pages Map iþlemleri
app.MapControllers(); // API rotalarý için
app.MapRazorPages();  // Sayfa rotalarý için

app.Run();