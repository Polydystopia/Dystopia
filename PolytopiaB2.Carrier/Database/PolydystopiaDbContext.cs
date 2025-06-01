using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;

namespace PolytopiaB2.Carrier.Database;

public class PolydystopiaDbContext : DbContext
{
    public DbSet<PolytopiaUserViewModel> Users { get; set; }

    public PolydystopiaDbContext(DbContextOptions<PolydystopiaDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var serializerOptions = new System.Text.Json.JsonSerializerOptions();
        
        var polytopiaUserViewModelEntity = modelBuilder.Entity<PolytopiaUserViewModel>();

        polytopiaUserViewModelEntity.HasKey(e => e.PolytopiaId);
        
        polytopiaUserViewModelEntity.Property(e => e.Victories).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(v, serializerOptions));
        
        polytopiaUserViewModelEntity.Property(e => e.Defeats).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(v, serializerOptions));
        
        polytopiaUserViewModelEntity.Property(e => e.GameVersions).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<List<ClientGameVersionViewModel>>(v, serializerOptions));
        
        polytopiaUserViewModelEntity.Property(e => e.UnlockedTribes).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, serializerOptions));
        
        polytopiaUserViewModelEntity.Property(e => e.UnlockedSkins).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, serializerOptions));
        
        polytopiaUserViewModelEntity.Property(e => e.CmUserData).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<UserViewModel>(v, serializerOptions));
    }
}