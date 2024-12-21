using LakatosCardReader.CardReader;
using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Parsers;
using PCSC;
using Web.Hubs;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Registrovani class library servisi
builder.Services.AddSingleton<ILCardMonitor, LCardMonitor>();

builder.Services.AddSingleton<ILCardReader, LCardReader>();

builder.Services.AddSingleton<ILCardTypeParser, LCardTypeParser>();


builder.Services.AddSingleton<ILIdentityCardReader, LIdentityCardReader>();
builder.Services.AddSingleton<ILVehicleCardReader, LVehicleCardReader>();

builder.Services.AddSingleton<ILIdentityDataParser, LIdentityCardParser>();
builder.Services.AddSingleton<ILVehicleDataParser, LVehicleCardParser>();

//builder.Services.AddTransient<ICardService, CardService>();






builder.Services.AddSingleton<CardReaderService>();

builder.Services.AddSignalR();

var app = builder.Build();

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

app.MapHub<CardReaderHub>("/cardReaderHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
