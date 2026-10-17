using BirthdayTimeMachine.Components;
using BirthdayTimeMachine.Services;
using BirthdayTimeMachine.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Geocoding API - city name to coordinates
builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
{
    client.BaseAddress = new Uri("https://geocoding-api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Historical weather API - weather for a past date
builder.Services.AddHttpClient<IWeatherHistoryService, WeatherHistoryService>(client =>
{
    client.BaseAddress = new Uri("https://archive-api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<IOnThisDayService, OnThisDayService>(client =>
{
    client.BaseAddress = new Uri("https://en.wikipedia.org/");
    client.Timeout = TimeSpan.FromSeconds(10);
    // Wikipedia asks API users to identify their app
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BirthdayTimeMachine/1.0 (college student project)");
});

// NASA Astronomy Picture of the Day
builder.Services.AddHttpClient<INasaApodService, NasaApodService>(client =>
{
    client.BaseAddress = new Uri("https://science.nasa.gov/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BirthdayTimeMachine/1.0 (college student project)");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
