using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using PolytopiaA10.Carrier.Hubs.ModifiedProtocol;
using PolytopiaB2.Carrier.Hubs;
using PolytopiaB2.Carrier.Patches;
using UnityEngine;

TimeHook.Initialize();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR().AddNewtonsoftJsonAotProtocol();

var app = builder.Build();
app.MapControllers();
app.MapHub<PolytopiaHub>("/gamehub");

app.MapGet("/", () => "Hello World!");

Log.AddLogger(new MyLogger());

var harmony = new Harmony("carrier");
harmony.PatchAll();

app.Run();
