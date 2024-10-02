using Blazor;
using Blazor.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient("std", op =>
{
    op.BaseAddress = ApiAddress.Uri;
});

builder.Services.AddHttpClient<AuthenticationService>();
builder.Services.AddHttpClient<IWeatherService, ClientWeatherService>();
builder.Services.AddHttpClient<IWeatherService, AnotherClientWeatherService>();

builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

//builder.Services.AddTransient<IWeatherService, ClientWeatherService>();
builder.Services.AddTransient<IWeatherService, AnotherClientWeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
