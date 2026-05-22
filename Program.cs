using InvestTracker.Components;
using InvestTracker.Data;
using InvestTracker.Services.Interfaces;
using InvestTracker.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using InvestTracker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));

// ⚠️ Ordre important !
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAssetService, AssetService>();
// Portfolio AVANT Transaction car Transaction dépend de Portfolio
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddScoped<AuthState>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DbSeeder.SeedAsync(context);

        // ── CORRECTION : reconstruire tous les portefeuilles
        // au démarrage pour corriger les suppressions passées
        // qui n'avaient pas mis à jour le portefeuille.
        // À retirer après la première exécution si vous voulez,
        // mais ça ne coûte rien de le garder (très rapide).
        var portfolioService = services
            .GetRequiredService<IPortfolioService>();
        var userIds = context.Users.Select(u => u.Id).ToList();
        foreach (var uid in userIds)
        {
            await portfolioService.RebuildPortfolioAsync(uid);
            Console.WriteLine($"✅ Portefeuille reconstruit pour userId={uid}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur : {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();