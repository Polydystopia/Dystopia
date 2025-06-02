using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PolytopiaB2.Carrier.Database.Friendship;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaB2.Carrier.Database;

public class PolydystopiaDbContext : DbContext
{
    public DbSet<PolytopiaUserViewModel> Users { get; set; }
    public DbSet<FriendshipEntity> Friends { get; set; }
    public DbSet<LobbyGameViewModel> Lobbies { get; set; }

    public PolydystopiaDbContext(DbContextOptions<PolydystopiaDbContext> options) : base(options)
    {
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
        
        
        
        var friendshipEntity = modelBuilder.Entity<FriendshipEntity>();
    
        friendshipEntity.HasKey(e => new { e.UserId1, e.UserId2 });
    
        friendshipEntity
            .HasOne(f => f.User1)
            .WithMany()
            .HasForeignKey(f => f.UserId1);
        
        friendshipEntity
            .HasOne(f => f.User2)
            .WithMany()
            .HasForeignKey(f => f.UserId2);

        
        
        var lobbyEntity = modelBuilder.Entity<LobbyGameViewModel>();

        lobbyEntity.HasKey(e => e.Id);
        
        // Configure GameContext as JSON since it's a complex object
        lobbyEntity.Property(e => e.GameContext).HasConversion(
            v => v != null ? System.Text.Json.JsonSerializer.Serialize(v, serializerOptions) : null,
            v => !string.IsNullOrEmpty(v) ? System.Text.Json.JsonSerializer.Deserialize<GameContext>(v, serializerOptions) : null);
        
        // Configure JSON serialization for complex properties
        lobbyEntity.Property(e => e.DisabledTribes).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, serializerOptions));
        
        lobbyEntity.Property(e => e.Participators).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<List<ParticipatorViewModel>>(v, serializerOptions));
        
        lobbyEntity.Property(e => e.Bots).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, serializerOptions),
            v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, serializerOptions));

    }
}