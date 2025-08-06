using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.News;
using Dystopia.Database.Replay;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PolytopiaBackendBase.Auth;

namespace Dystopia.Database;

public class PolydystopiaDbContext : DbContext
{
    public virtual DbSet<UserEntity> Users { get; set; }
    public virtual DbSet<FriendshipEntity> Friends { get; set; }
    public virtual DbSet<LobbyEntity> Lobbies { get; set; }
    public virtual DbSet<GameEntity> Games { get; set; }
    public virtual DbSet<MatchmakingEntity> Matchmaking { get; set; }
    public virtual DbSet<NewsEntity> News { get; set; }
    public DbSet<UserFavoriteGame> UserFavoriteGames { get; set; }

    public DbSet<LobbyParticipatorUserEntity> LobbyParticipators { get; set; }
    public DbSet<GameParticipatorUserEntity> GameParticipators  { get; set; }

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

        var userEntity = modelBuilder.Entity<UserEntity>();

        userEntity.HasKey(e => e.Id);

        userEntity.Property(e => e.GameVersions).HasConversion(
            v => JsonConvert.SerializeObject
                (v, jsonSettings),
            v => JsonConvert.DeserializeObject
                <List<ClientGameVersionViewModel>>(v, jsonSettings));

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

        lobbyEntity.Property(e => e.Bots).HasConversion(
            v => JsonConvert.SerializeObject(v, jsonSettings),
            v => JsonConvert.DeserializeObject<List<int>>(v, jsonSettings));

        #endregion

        #region Game

        var gameEntity = modelBuilder.Entity<GameEntity>();

        gameEntity.HasKey(e => e.Id);

        gameEntity.Property(e => e.TimerSettings).HasConversion(
            v => v != null ? JsonConvert.SerializeObject(v, jsonSettings) : null,
            v => !string.IsNullOrEmpty(v)
                ? JsonConvert.DeserializeObject<PolytopiaBackendBase.Timers.TimerSettings>(v, jsonSettings)
                : null);

        gameEntity.HasOne(g => g.Owner).WithMany().HasForeignKey(g => g.OwnerId);

        #endregion

        #region Participation

        #region Participation — Lobby

        modelBuilder.Entity<LobbyParticipatorUserEntity>(b =>
        {
            b.HasKey(lp => new { lp.LobbyId, lp.UserId });

            b.HasOne(lp => lp.Lobby)
                .WithMany(l => l.Participators)
                .HasForeignKey(lp => lp.LobbyId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(lp => lp.User)
                .WithMany(u => u.LobbyParticipations)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region Participation — Game

        modelBuilder.Entity<GameParticipatorUserEntity>(b =>
        {
            b.HasKey(gp => new { gp.GameId, gp.UserId });

            b.HasOne(gp => gp.Game)
                .WithMany(g => g.Participators)
                .HasForeignKey(gp => gp.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(gp => gp.User)
                .WithMany(u => u.GameParticipations)
                .HasForeignKey(gp => gp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

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