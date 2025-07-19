using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.News;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Database;

public class PolydystopiaDbContext : DbContext
{
    public virtual DbSet<UserEntity> Users { get; init; }
    public virtual DbSet<FriendshipEntity> Friends { get; init; }
    public virtual DbSet<LobbyEntity> Lobbies { get; init; }
    public virtual DbSet<GameEntity> Games { get; init; }
    public virtual DbSet<MatchmakingEntity> Matchmaking { get; init; }
    public virtual DbSet<NewsEntity> News { get; init; }

    public PolydystopiaDbContext(DbContextOptions<PolydystopiaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jsonSettings = new JsonSerializerSettings
        {
            // This handles private setters
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
        
        #region userEntity
        var userEntity = modelBuilder.Entity<UserEntity>();
        userEntity
            .HasMany(u => u.Friends1)
            .WithOne(f => f.User1)
            .HasForeignKey(f => f.UserId1)
            .OnDelete(DeleteBehavior.Restrict);
        userEntity
            .HasMany(u => u.Friends2)
            .WithOne(f => f.User2)
            .HasForeignKey(f => f.UserId2)
            .OnDelete(DeleteBehavior.Restrict);
        #endregion
        
        #region friendshipEntity
        var friendshipEntity = modelBuilder.Entity<FriendshipEntity>();
        
        #endregion
        
        #region UserLobbyEntity
        modelBuilder.Entity<LobbyPlayerEntity>()
            .HasOne(lp => lp.User)
            .WithMany(u => u.Games)
            .HasForeignKey(ul => ul.UserId);
        modelBuilder.Entity<LobbyPlayerEntity>()
            .HasOne(ul => ul.Lobby)
            .WithMany(l => l.Participators)
            .HasForeignKey(ul => ul.LobbyId);
        #endregion
        #region lobbyEntity
        var lobbyEntity = modelBuilder.Entity<LobbyEntity>();
        
        lobbyEntity
            .HasOne(l => l.StartedGame)
            .WithOne(g => g.Lobby)
            .HasForeignKey<LobbyEntity>(g => g.StartedGameId);
        
        lobbyEntity
            .HasOne(l => l.MatchmakingGame)
            .WithOne(m => m.LobbyGameViewModel)
            .HasForeignKey<LobbyEntity>(l => l.MatchmakingGameId);
        
        lobbyEntity.Property(e => e.Bots).HasConversion(
            v => JsonConvert.SerializeObject(v, jsonSettings),
            v => JsonConvert.DeserializeObject<List<int>>(v, jsonSettings)!);
        #endregion
        
        #region gameEntity
        var gameEntity = modelBuilder.Entity<GameEntity>();

        gameEntity
            .HasOne(g => g.Owner)
            .WithMany() // Assuming no collection of owned games on UserEntity
            .HasForeignKey(g => g.OwnerId);
        
        //TODO
        gameEntity.Property(e => e.TimerSettings).HasConversion(
            v => v != null ? JsonConvert.SerializeObject(v, jsonSettings) : null,
            v => !string.IsNullOrEmpty(v) ? JsonConvert.DeserializeObject<PolytopiaBackendBase.Timers.TimerSettings>(v, jsonSettings) : null!);
        #endregion

        #region matchmakingEntity
        var matchmakingEntity = modelBuilder.Entity<MatchmakingEntity>();

        matchmakingEntity.HasKey(e => e.Id);

        matchmakingEntity
            .HasOne(m => m.LobbyGameViewModel)
            .WithMany()
            .HasForeignKey(m => m.LobbyGameViewModelId)
            .OnDelete(DeleteBehavior.Cascade);

        
        #endregion
    }
}