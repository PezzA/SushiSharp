using Akka.Actor;
using Akka.Hosting;

using BoardCutter.Core.Actors;
using BoardCutter.Core.Actors.HubWriter;
using BoardCutter.Core.Web.Components.Account;
using BoardCutter.Core.Web.Shared.Chat;
using BoardCutter.Core.Data;
using BoardCutter.Core.Players;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using BoardCutter.Games.SushiGo;
using BoardCutter.Games.SushiGo.Actors.Game;
using BoardCutter.Games.SushiGo.Scoring;
using BoardCutter.Games.SushiGo.Shufflers;
using BoardCutter.Games.Twenty48;
using BoardCutter.Web.Components;
using BoardCutter.Web.Components.Account;
using BoardCutter.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddSingleton<IPlayerService, MemoryPlayerService>();
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
            Dictionary<CardType, IScorer> scorers = new()
            {
                { CardType.Dumpling, new DumplingScorer() },
                { CardType.MakiRolls, new MakiRollScorer() },
                { CardType.Nagiri, new NagiriScorer() },
                { CardType.Pudding, new PuddingScorer() },
                { CardType.Sashimi, new SashimiScorer() },
                { CardType.Tempura, new TempuraScorer() },
            };

            var sushiHubWriter =
                system.ActorOf(
                    Props.Create(() =>
                        new HubClientWriter<SushiHub>(
                            resolver.GetService<IHubContext<SushiHub>>(),
                            resolver.GetService<IPlayerService>())),
                    "SushiHubWriter");
            
            
            var twenty48HubWriter =
                system.ActorOf(
                    Props.Create(() =>
                        new HubClientWriter<Twenty48Hub>(
                            resolver.GetService<IHubContext<Twenty48Hub>>(),
                            resolver.GetService<IPlayerService>())),
                    "2048HubWriter");

            var gameActors = new Dictionary<string, Props>
            {
                {
                    "SushiGo",
                    Props.Create(() => new GameActor(resolver.GetService<ICardShuffler>(), scorers, sushiHubWriter))
                },
                {
                    "2048",
                    
                    Props.Create(() =>  new BoardCutter.Games.Twenty48.Actors.GameActor(twenty48HubWriter, new RandomTilePlacer()))
                }
            };
            
            var gameManagerActor =
                system.ActorOf(
                    Props.Create(
                        () => new GameManager(gameActors)),
                    "GameManagerActor");

            registry.Register<GameManager>(gameManagerActor);
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

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSingleton<IChatService, MemoryChatService>();
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
app.MapHub<Twenty48Hub>("/2048hub");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
}

app.Run();