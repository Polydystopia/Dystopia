using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using HarmonyLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonoMod.RuntimeDetour;
using PolytopiaA10.Carrier.Hubs.ModifiedProtocol;
using Dystopia.Bridge;
using Dystopia.Database;
using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Highscore;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.News;
using Dystopia.Database.User;
using Dystopia.Hubs;
using Dystopia.Info;
using Dystopia.Managers.Game;
using Dystopia.Managers.Highscore;
using Dystopia.Patches;
using Dystopia.Services.Cache;
using Dystopia.Services.News;
using Dystopia.Services.Steam;
using Dystopia.Settings;
using DystopiaShared;
using Microsoft.Extensions.Options;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using Serilog;
using UnityEngine;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db"));
var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "polytopia.db");
builder.Services.AddDbContext<PolydystopiaDbContext>(options =>
    options.UseLazyLoadingProxies().UseSqlite($"Data Source={dbPath}"));

builder.Logging.ClearProviders();

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/app-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 10_000_000,
            rollOnFileSizeLimit: true,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1))
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PolytopiaB2.Carrier"));


builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Issuer",
            ValidAudience = "Audience",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("higul0u9pgwojaingwagvupöjoahg8wag890zuahgvbuaagau9j")), //TODO: Other key
            NameClaimType = "unique_name",
        };

        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(new CustomJwtSecurityTokenHandler());

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) &&
                    authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var accessToken = authHeader.Substring("Bearer ".Length).Trim();

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/gamehub"))
                    {
                        context.Token = accessToken;
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddSignalR().AddNewtonsoftJsonAotProtocol();

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
builder.Services.Configure<SteamSettings>(builder.Configuration.GetSection("Steam"));
builder.Services.Configure<Il2cppSettings>(builder.Configuration.GetSection("Il2cppSettings"));

#region repository
builder.Services.AddScoped<IPolydystopiaUserRepository, PolydystopiaUserRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IPolydystopiaLobbyRepository, PolydystopiaLobbyRepository>();
builder.Services.AddScoped<IPolydystopiaGameRepository, PolydystopiaGameRepository>();
builder.Services.AddScoped<IPolydystopiaMatchmakingRepository, PolydystopiaMatchmakingRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IDystopiaHighscoreRepository, DystopiaHighscoreRepository>();
#endregion

builder.Services.AddSingleton<INewsService, NewsService>();

#region cache

builder.Services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));
builder.Services.AddHostedService<CacheCleaningService>();

#endregion

#region manager
builder.Services.AddScoped<IPolydystopiaGameManager, PolydystopiaGameManager>();
builder.Services.AddScoped<IDystopiaHighscoreManager, DystopiaHighscoreManager>();
#endregion

builder.Services.AddScoped<IDystopiaCastle, DystopiaBridge>();
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("GameCacheSettings"));
builder.Services.Configure<SteamSettings>(
    builder.Configuration.GetSection("Steam"));
builder.Services.AddScoped<ISteamService, SteamService>();

builder.Services.Configure<InstanceInfoSettings>(builder.Configuration.GetSection("InstanceInfo"));
builder.Services.AddSingleton(new StartTimeHolder { StartTime = DateTime.UtcNow });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DystopiaHub>("/gamehub");

Log.AddLogger(new MyLogger());

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PolydystopiaDbContext>();

    dbContext.Database.Migrate();
}

var il2CPPSettings = app.Services.GetRequiredService<IOptions<Il2cppSettings>>().Value;
DystopiaBridge.InitIl2Cpp(!il2CPPSettings.Enabled);

PolytopiaDataManager.provider = new MyProvider();

var gameCache = app.Services.GetRequiredService<ICacheService<GameEntity>>();
GameCache.InitializeCache(gameCache);

app.Run();

public class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
{
    public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters,
        out SecurityToken validatedToken)
    {
        var principal = base.ValidateToken(token, validationParameters, out validatedToken);

        var jwtToken = validatedToken as JwtSecurityToken;
        if (jwtToken != null)
        {
            var identity = principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                foreach (var claim in jwtToken.Claims)
                {
                    if (!identity.HasClaim(c => c.Type == claim.Type))
                    {
                        identity.AddClaim(new Claim(claim.Type, claim.Value));
                    }
                }
            }
        }

        return principal;
    }
}