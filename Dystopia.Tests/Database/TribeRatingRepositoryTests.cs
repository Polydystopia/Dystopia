using Dystopia.Database;
using Dystopia.Database.TribeRating;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Polytopia.Data;
using Xunit;

namespace Dystopia.Tests.Database;

public class TribeRatingRepositoryTests
{
    private readonly DbContextOptions<PolydystopiaDbContext> _options =
        new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<PolydystopiaDbContext> _dbContextMock;

    public TribeRatingRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);
    }

    private DystopiaTribeRatingRepository CreateRepository() => new(_dbContextMock.Object);

    private UserEntity CreateUserEntity(Guid? id = null, string userName = "TestUser")
    {
        return new UserEntity
        {
            Id = id ?? Guid.NewGuid(),
            SteamId = "123456789",
            UserName = userName,
            Discriminator = "1234",
            CurrentLeagueId = 1
        };
    }

    private TribeRatingEntity CreateTribeRatingEntity(
        Guid? userId = null,
        TribeData.Type tribe = TribeData.Type.Xinxi,
        uint? score = null,
        uint? rating = null,
        UserEntity user = null)
    {
        return new TribeRatingEntity
        {
            UserId = userId ?? Guid.NewGuid(),
            Tribe = tribe,
            Score = score,
            Rating = rating,
            User = user ?? CreateUserEntity(userId)
        };
    }

    #region AI

    [Fact]
    public async Task GetByUserAsync_ReturnsUserTribeRatings_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserEntity(userId);
        var tribeRatings = new List<TribeRatingEntity>
        {
            CreateTribeRatingEntity(userId, TribeData.Type.Xinxi, 1500, 1200, user),
            CreateTribeRatingEntity(userId, TribeData.Type.Bardur, 2000, 1400, user),
            CreateTribeRatingEntity(userId, TribeData.Type.Imperius, 1000, 1100, user)
        };

        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, tr => Assert.Equal(userId, tr.UserId));
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsEmptyList_WhenNoRatingsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribeRatings = new List<TribeRatingEntity>();

        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddOrUpdateAsync_CreatesNewTribeRating_WhenNotExists()
    {
        // Arrange
        var newTribeRating = CreateTribeRatingEntity(score: 1500, rating: 1200);
        var emptyTribeRatings = new List<TribeRatingEntity>();

        var mockDbSet = emptyTribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(x => x.Add(newTribeRating), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpdatesBothValues_WhenNewValuesAreHigher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1000, rating: 1100);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1300);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.True(result);
        Assert.Equal(1500u, existingTribeRating.Score);
        Assert.Equal(1300u, existingTribeRating.Rating);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        mockDbSet.Verify(x => x.Add(It.IsAny<TribeRatingEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpdatesOnlyScore_WhenOnlyScoreIsHigher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1000, rating: 1300);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1200);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.True(result);
        Assert.Equal(1500u, existingTribeRating.Score);
        Assert.Equal(1300u, existingTribeRating.Rating); // Should remain unchanged
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpdatesOnlyRating_WhenOnlyRatingIsHigher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1100);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1200, rating: 1300);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.True(result);
        Assert.Equal(1500u, existingTribeRating.Score); // Should remain unchanged
        Assert.Equal(1300u, existingTribeRating.Rating);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAsync_DoesNotUpdate_WhenNewValuesAreLower()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 2000, rating: 1500);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1200);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.False(result);
        Assert.Equal(2000u, existingTribeRating.Score);
        Assert.Equal(1500u, existingTribeRating.Rating);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        mockDbSet.Verify(x => x.Add(It.IsAny<TribeRatingEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateAsync_DoesNotUpdate_WhenNewValuesAreEqual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1200);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1200);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.False(result);
        Assert.Equal(1500u, existingTribeRating.Score);
        Assert.Equal(1200u, existingTribeRating.Rating);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        mockDbSet.Verify(x => x.Add(It.IsAny<TribeRatingEntity>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateAsync_HandlesNullValues_InExistingEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: null, rating: null);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1200);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.True(result);
        Assert.Equal(1500u, existingTribeRating.Score);
        Assert.Equal(1200u, existingTribeRating.Rating);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAsync_HandlesNullValues_InNewEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: 1200);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: null, rating: null);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.False(result);
        Assert.Equal(1500u, existingTribeRating.Score);
        Assert.Equal(1200u, existingTribeRating.Rating);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpdatesPartially_WhenOneValueIsHigherAndOneIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1000, rating: 1200);
        var newTribeRating = CreateTribeRatingEntity(userId, tribe, score: 1500, rating: null);

        var tribeRatings = new List<TribeRatingEntity> { existingTribeRating };
        var mockDbSet = tribeRatings.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.TribeRatings).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().AddOrUpdateAsync(newTribeRating);

        // Assert
        Assert.True(result);
        Assert.Equal(1500u, existingTribeRating.Score);
        Assert.Equal(1200u, existingTribeRating.Rating); // Should remain unchanged
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    #endregion
}