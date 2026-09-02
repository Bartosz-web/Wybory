using Microsoft.EntityFrameworkCore;
using Wybory.Web.Dane;
using Wybory.Web.Uslugi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<BazaDanych>(opcje =>
    opcje.UseSqlite(builder.Configuration.GetConnectionString("BazaWyborow")));

builder.Services.AddScoped<UslugaRejestracji>();
builder.Services.AddScoped<UslugaGlosowania>();
builder.Services.AddScoped<UslugaZliczania>();
builder.Services.AddScoped<UslugaSymulacji>();

var app = builder.Build();

// Tworzy bazę (migracje) i zakłada 3 okręgi wyborcze przy 1. starcie (pkt 0).
InicjalizatorBazy.Inicjalizuj(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Kody odpowiedzi bez wyjątku, przede wszystkim 404.
app.UseStatusCodePagesWithReExecute("/Blad/{0}");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
