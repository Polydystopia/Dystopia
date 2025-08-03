using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.News;
using Dystopia.Database.Replay;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Database;

public class PolydystopiaDbContext : DbContext
{
    public virtual DbSet<PolytopiaUserViewModel> Users { get; set; }
    public virtual DbSet<FriendshipEntity> Friends { get; set; }
    public virtual DbSet<LobbyEntity> Lobbies { get; set; }
    public virtual DbSet<GameEntity> Games { get; set; }
    public virtual DbSet<MatchmakingEntity> Matchmaking { get; set; }
    public virtual DbSet<NewsEntity> News { get; set; }
    public DbSet<UserFavoriteGame> UserFavoriteGames { get; set; }

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

        #region User

        var polytopiaUserViewModelEntity = modelBuilder.Entity<PolytopiaUserViewModel>();

        polytopiaUserViewModelEntity.HasKey(e => e.PolytopiaId);

        polytopiaUserViewModelEntity.Property<string>("UserName");
        polytopiaUserViewModelEntity.Property<string>("Alias");

        polytopiaUserViewModelEntity.Property(e => e.Victories).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <Dictionary<string, int>>(v, jsonSettings));

        polytopiaUserViewModelEntity.Property(e => e.Defeats).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <Dictionary<string, int>>(v, jsonSettings));

        polytopiaUserViewModelEntity.Property(e => e.GameVersions).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <List<ClientGameVersionViewModel>>(v, jsonSettings));

        polytopiaUserViewModelEntity.Property(e => e.UnlockedTribes).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <List<int>>(v, jsonSettings));

        polytopiaUserViewModelEntity.Property(e => e.UnlockedSkins).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <List<int>>(v, jsonSettings));

        polytopiaUserViewModelEntity.Property(e => e.CmUserData).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <UserViewModel>(v, jsonSettings));

        #endregion

        #region Friendship

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

        #endregion

        #region Lobby

        var lobbyEntity = modelBuilder.Entity<LobbyEntity>();

        lobbyEntity.HasKey(e => e.Id);

        lobbyEntity.Property(e => e.DisabledTribes).HasConversion(
            v => JsonConvert.SerializeObject(v, jsonSettings),
            v => JsonConvert.DeserializeObject<List<int>>(v, jsonSettings));

        lobbyEntity.Property(e => e.Participators).HasConversion(
            v => JsonConvert.SerializeObject(v, jsonSettings),
            v => JsonConvert.DeserializeObject<List<ParticipatorViewModel>>(v, jsonSettings));

        lobbyEntity.Property(e => e.Bots).HasConversion(
            v => JsonConvert.SerializeObject(v, jsonSettings),
            v => JsonConvert.DeserializeObject<List<int>>(v, jsonSettings));

        #endregion

        #region Game

        var gameEntity = modelBuilder.Entity<GameEntity>();

        gameEntity.HasKey(e => e.Id);

        gameEntity.Property(e => e.TimerSettings).HasConversion(
            v => v != null ? JsonConvert.SerializeObject(v, jsonSettings) : null,
            v => !string.IsNullOrEmpty(v) ? JsonConvert.DeserializeObject<PolytopiaBackendBase.Timers.TimerSettings>(v, jsonSettings) : null);

        #endregion

        #region Matchmaking

        var matchmakingEntity = modelBuilder.Entity<MatchmakingEntity>();

        matchmakingEntity.HasKey(e => e.Id);

        matchmakingEntity
            .HasOne(m => m.LobbyEntity)
            .WithMany()
            .HasForeignKey(m => m.LobbyEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        matchmakingEntity.Property(e => e.PlayerIds).HasConversion(
            v => JsonConvert.SerializeObject(v, jsonSettings),
            v => JsonConvert.DeserializeObject<List<Guid>>(v, jsonSettings));

        #endregion

        #region Favorite

        var favEntity = modelBuilder.Entity<UserFavoriteGame>();

        favEntity.HasKey(uf => new { uf.UserId, uf.GameId });

        favEntity
            .HasOne(uf => uf.User)
            .WithMany()
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        favEntity
            .HasOne(uf => uf.Game)
            .WithMany()
            .HasForeignKey(uf => uf.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}