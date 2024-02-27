using Akka.Actor;
using Akka.Hosting;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using SushiSharp.Cards.Shufflers;
using SushiSharp.Game;
using SushiSharp.Game.Chat;
using SushiSharp.Web.Actors;
using SushiSharp.Web.Hubs;
using SushiSharp.Web.Components;
using SushiSharp.Web.Components.Account;
using SushiSharp.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

//builder.Services.AddResponseCompression(opts =>
//{
//    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
//        new[] { "application/octet-stream" });
//});

builder.Services.AddAkka("MyActorSystem", configurationBuilder =>
{
    configurationBuilder
        .WithActors((system, registry, resolver) =>
        {
            var consoleActor = system.ActorOf(Props.Create(() => new ConsoleActor(resolver.GetService<IHubContext<LobbyHub>>())), "console");
            registry.Register<ConsoleActor>(consoleActor);
        });
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSingleton<IChatService, MemoryChatService>();
builder.Services.AddSingleton<IGameService, MemoryGameService>();
builder.Services.AddSingleton<IPlayerService, MemoryPlayerService>();
builder.Services.AddSingleton<ICardShuffler, RandomCardShuffler>();

builder.Services.AddMudServices();

var app = builder.Build();

//app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<LobbyHub>("/lobbyhub");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();