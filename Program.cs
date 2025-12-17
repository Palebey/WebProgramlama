using Microsoft.AspNetCore.Identity; // Identity kütüphanesi eklendi
using Microsoft.EntityFrameworkCore;
using web.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý
builder.Services.AddDbContext<SporSalonuDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SporSalonuConnection")));

// 2. Identity Servislerinin Eklenmesi (Giriþ/Çýkýþ ve Rol Yönetimi)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Ödev için basit þifre kurallarý (Karmaþýk þifre zorunluluðunu kaldýrýyoruz)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3; // "sau" þifresi 3 karakter olduðu için

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SporSalonuDbContext>()
.AddDefaultTokenProviders();

// 3. Giriþ Yapýlmamýþsa Yönlendirme Ayarlarý
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Giriþ sayfasý yolu
    options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eriþim sayfasý
});

// 4. Razor Pages Ayarlarý
builder.Services.AddRazorPages(options =>
{
    // Admin klasörüne sadece giriþ yapmýþ olanlar eriþebilir.
    // Ýleride burayý sadece "Admin" rolüne sahip olanlar olarak güncelleyebiliriz.
    options.Conventions.AuthorizeFolder("/Admin");
});

var app = builder.Build();

// --- ÖDEV GEREKSÝNÝMÝ: OTOMATÝK ADMÝN VE ROL OLUÞTURMA ---
// Proje her baþladýðýnda bu kýsým çalýþýr, veritabanýnda Admin yoksa ekler.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Rolleri oluþtur (Admin ve Uye)
        string[] roles = { "Admin", "Uye" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin Kullanýcýsýný Oluþtur
        var adminEmail = "b221210003@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            // Þifre: sau
            var result = await userManager.CreateAsync(newAdmin, "sau");

            if (result.Succeeded)
            {
                // Kullanýcýya Admin rolünü ata
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        // Hata olursa konsola yazdýr
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

// 5. ÖNEMLÝ: Authentication (Kimlik Doðrulama) Authorization'dan ÖNCE gelmelidir.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();