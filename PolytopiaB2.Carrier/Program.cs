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
using PolytopiaB2.Carrier.Database;
using PolytopiaB2.Carrier.Database.Friendship;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Database.Lobby;
using PolytopiaB2.Carrier.Database.User;
using PolytopiaB2.Carrier.Hubs;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using UnityEngine;

TimeHook.Initialize();

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "polytopia.db");
builder.Services.AddDbContext<PolydystopiaDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

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

builder.Services.AddScoped<IPolydystopiaUserRepository, PolydystopiaUserRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IPolydystopiaLobbyRepository, PolydystopiaLobbyRepository>();
builder.Services.AddScoped<IPolydystopiaGameRepository, PolydystopiaGameRepository>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PolytopiaHub>("/gamehub");

app.MapGet("/", () => "Hello World!");

Log.AddLogger(new MyLogger());

var harmony = new Harmony("carrier");
harmony.PatchAll();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PolydystopiaDbContext>();
    
    dbContext.Database.Migrate();
}

app.Run();

public class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
{
    public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
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
