using KsSelect.Samples.Infrastructure;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Db Context
builder.Services.AddKsSelectContext(builder.Configuration);

// Add Repository registration
builder.Services.AddRepositories();

builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var dataContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
	await dataContext.Database.MigrateAsync();
}

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

app.UseAuthorization();

var supportedCultures = new[] { "en", "it" };
app.UseRequestLocalization(new RequestLocalizationOptions()
	.SetDefaultCulture(supportedCultures[0])
	.AddSupportedCultures(supportedCultures)
	.AddSupportedUICultures(supportedCultures)
	.AddInitialRequestCultureProvider(new RouteDataRequestCultureProvider()));

app.MapControllerRoute(
	name: "default",
	pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");

app.Run();
