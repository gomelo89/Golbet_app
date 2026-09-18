using GolBet.Repositories.Data;
using Microsoft.EntityFrameworkCore;
using GolBet.Repositories.Implementations;
using GolBet.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── 1. REGISTRO DE SERVICIOS (Siempre ANTES de builder.Build()) ──
builder.Services.AddControllersWithViews();

// Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorio genérico
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Repositorios específicos
builder.Services.AddScoped<IMatchRepository, MatchRepository>();


// ── 2. CONSTRUIR LA APLICACIÓN ──
var app = builder.Build();


// ── 3. EJECUCIÓN EN TIEMPO DE ARRANQUE Y PIPELINE HTTP ──
// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();