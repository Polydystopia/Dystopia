using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Highscore;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.News;
using Dystopia.Database.TribeRating;
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
    public virtual DbSet<HighscoreEntity> Highscores { get; set; }
    public virtual DbSet<TribeRatingEntity> TribeRatings { get; set; }

    public DbSet<LobbyParticipatorUserEntity> LobbyParticipators { get; set; }
    public DbSet<GameParticipatorUserUser> GameParticipators { get; set; }

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

        userEntity.HasMany(u => u.FavoriteGames).WithMany().UsingEntity(j => j.ToTable("UserFavoriteGames"));

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

        lobbyEntity.HasOne(g => g.Owner).WithMany().HasForeignKey(g => g.OwnerId);

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

        gameEntity.HasOne(g => g.Lobby)
            .WithOne("Game")
            .HasForeignKey<GameEntity>(g => g.LobbyId)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<GameParticipatorUserUser>(b =>
        {
            b.HasKey(gp => new { gp.GameId, gp.UserId });

            b.HasOne<GameEntity>("Game")
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

        #region Highscore

        var highscoreEntity = modelBuilder.Entity<HighscoreEntity>();

        highscoreEntity.HasKey(h => new { h.UserId, h.Tribe });
        highscoreEntity.HasOne(h => h.User).WithMany(h => h.Highscores).HasForeignKey(h => h.UserId);

        #endregion

        #region TribeRating

        var tribeRatingEntity = modelBuilder.Entity<TribeRatingEntity>();

        tribeRatingEntity.HasKey(tr => new { tr.UserId, tr.Tribe });
        tribeRatingEntity.HasOne(tr => tr.User).WithMany(h => h.TribeRatings).HasForeignKey(tr => tr.UserId);

        #endregion
    }
}