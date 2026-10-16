using GLMS.Data;
using GLMS.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<GlmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.BaseAddress = new Uri("https://open.er-api.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
var storageProvider = builder.Configuration["Storage:Provider"];
if (string.Equals(storageProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["Storage:BlobConnectionString"]))
        throw new InvalidOperationException("Configure Storage:BlobConnectionString before selecting AzureBlob storage.");
    builder.Services.AddSingleton<IAgreementStorage, AzureBlobAgreementStorage>();
}
else
{
    builder.Services.AddSingleton<IAgreementStorage, LocalAgreementStorage>();
}

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<GlmsDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
