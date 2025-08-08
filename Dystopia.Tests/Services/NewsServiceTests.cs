using Microsoft.Extensions.DependencyInjection;

namespace Dystopia.Tests.Services;

using Moq;
using Xunit;
using Dystopia.Services.News;
using Dystopia.Database.News;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

public class NewsServiceTests
{
    private readonly Mock<INewsRepository> _mockRepo;
    private readonly NewsService _newsService;

    public NewsServiceTests()
    {
        _mockRepo = new Mock<INewsRepository>();
        var services = new ServiceCollection();
        services.AddScoped<INewsRepository>(x => _mockRepo.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        _newsService = new NewsService(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    [Fact]
    public async Task GetNews_WhenNewsExist_ReturnsMappedNews()
    {
        // Arrange
        var testNews = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Test1", Link = "link1", Image = "image1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now},
            new() { Id = 2, Body = "Test2", Link = "link2", Image = "image2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(testNews);

        // Act
        var result = await _newsService.GetNews();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result.First().Id);
        Assert.Equal("Test1", result.First().Body);
        Assert.Equal("link1", result.First().Link);
        Assert.Equal("image1", result.First().Image);
    }

    [Fact]
    public async Task GetNews_WhenNoNews_ReturnsEmptyList()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());

        // Act
        var result = await _newsService.GetNews();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageExists_ReturnsMessageWithGuid()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("test");
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());
        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.StartsWith("test", result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageIsEmpty_ReturnsEmptyString()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("");
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());

        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageIsNull_ReturnsEmptyString()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync((string?) null);
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());

        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetNews_VerifyUnixTimestampConversion()
    {
        // Arrange
        var testEntity = new NewsEntity
        {
            Id = 0,
            NewsType = NewsType.News,
            Body = "null",
            Link = "test",
            Image = "null",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today,
            IsActive = true
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity> { testEntity });

        // Act
        var result = await _newsService.GetNews();

        // Assert
        var expectedTimestamp = ((DateTimeOffset)testEntity.CreatedAt).ToUnixTimeSeconds();
        Assert.Equal(expectedTimestamp, result.First().Date);
    }

    #region AI

    [Fact]
    public async Task Reinitialize_RefreshesNewsData()
    {
        // Arrange
        var initialNews = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Initial", Link = "link1", Image = "image1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        var updatedNews = new List<NewsEntity>
        {
            new() { Id = 2, Body = "Updated", Link = "link2", Image = "image2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };

        _mockRepo.SetupSequence(r => r.GetActiveNewsAsync())
            .ReturnsAsync(initialNews)
            .ReturnsAsync(updatedNews);
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("");

        // Act - First call should get initial data
        var initialResult = await _newsService.GetNews();

        // Reinitialize and get updated data
        await _newsService.Reinitialize();
        var updatedResult = await _newsService.GetNews();

        // Assert
        Assert.Single(initialResult);
        Assert.Equal("Initial", initialResult.First().Body);

        Assert.Single(updatedResult);
        Assert.Equal("Updated", updatedResult.First().Body);
    }

    [Fact]
    public async Task Reinitialize_RefreshesSystemMessage()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());
        _mockRepo.SetupSequence(r => r.GetSystemMessageAsync())
            .ReturnsAsync("Initial message")
            .ReturnsAsync("Updated message");

        // Act
        var initialMessage = await _newsService.GetSystemMessage();
        await _newsService.Reinitialize();
        var updatedMessage = await _newsService.GetSystemMessage();

        // Assert
        Assert.Equal("Initial message", initialMessage);
        Assert.Equal("Updated message", updatedMessage);
    }

    [Fact]
    public async Task GetNews_MultipleCallsWithoutReinitialize_ReturnsCachedData()
    {
        // Arrange
        var testNews = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Cached", Link = "link", Image = "image", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(testNews);

        // Act
        var result1 = await _newsService.GetNews();
        var result2 = await _newsService.GetNews();
        var result3 = await _newsService.GetNews();

        // Assert
        _mockRepo.Verify(r => r.GetActiveNewsAsync(), Times.Once);
        Assert.Same(result1, result2);
        Assert.Same(result2, result3);
    }

    [Fact]
    public async Task GetSystemMessage_MultipleCallsWithoutReinitialize_ReturnsCachedData()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("Cached message");

        // Act
        var result1 = await _newsService.GetSystemMessage();
        var result2 = await _newsService.GetSystemMessage();
        var result3 = await _newsService.GetSystemMessage();

        // Assert
        _mockRepo.Verify(r => r.GetSystemMessageAsync(), Times.Once);
        Assert.Equal("Cached message", result1);
        Assert.Equal("Cached message", result2);
        Assert.Equal("Cached message", result3);
    }

    [Fact]
    public async Task GetNews_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _newsService.GetNews());
    }

    [Fact]
    public async Task GetSystemMessage_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ThrowsAsync(new ArgumentException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _newsService.GetSystemMessage());
    }

    [Fact]
    public async Task Reinitialize_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ThrowsAsync(new TimeoutException("Timeout"));
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("test");

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => _newsService.Reinitialize());
    }

    [Fact]
    public async Task GetNews_WithDifferentNewsTypes_ReturnsAllActiveNews()
    {
        // Arrange
        var mixedNews = new List<NewsEntity>
        {
            new() { Id = 1, NewsType = NewsType.News, Body = "Regular News", Link = "link1", Image = "image1", CreatedAt = DateTime.Now, IsActive = true },
            new() { Id = 2, NewsType = NewsType.SystemMessage, Body = "System News", Link = "link2", Image = "image2", CreatedAt = DateTime.Now, IsActive = true }
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(mixedNews);

        // Act
        var result = await _newsService.GetNews();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, n => n.Body == "Regular News");
        Assert.Contains(result, n => n.Body == "System News");
    }

    [Fact]
    public async Task GetNews_WithNullLinkAndImage_MapsCorrectly()
    {
        // Arrange
        var newsWithNulls = new List<NewsEntity>
        {
            new() { Id = 1, Body = "No Link/Image", Link = null, Image = null, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(newsWithNulls);

        // Act
        var result = await _newsService.GetNews();

        // Assert
        Assert.Single(result);
        Assert.Null(result.First().Link);
        Assert.Null(result.First().Image);
    }

    [Fact]
    public async Task GetNews_WithDifferentDates_PreservesUnixTimestamps()
    {
        // Arrange
        var date1 = new DateTime(2023, 1, 1);
        var date2 = new DateTime(2023, 6, 15);
        var newsWithDates = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Old News", CreatedAt = date1, UpdatedAt = date1 },
            new() { Id = 2, Body = "Recent News", CreatedAt = date2, UpdatedAt = date2 }
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(newsWithDates);

        // Act
        var result = await _newsService.GetNews().ConfigureAwait(false);

        // Assert
        var sortedResult = result.OrderBy(n => n.Id).ToList();
        Assert.Equal(((DateTimeOffset)date1).ToUnixTimeSeconds(), sortedResult[0].Date);
        Assert.Equal(((DateTimeOffset)date2).ToUnixTimeSeconds(), sortedResult[1].Date);
    }

    [Fact]
    public async Task ConcurrentGetNews_ThreadSafe()
    {
        // Arrange
        var testNews = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Concurrent Test", Link = "link", Image = "image", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(testNews);

        // Act - Multiple concurrent calls
        var tasks = Enumerable.Range(0, 20).Select(_ => _newsService.GetNews()).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        _mockRepo.Verify(r => r.GetActiveNewsAsync(), Times.Once);
        Assert.All(results, result =>
        {
            Assert.Single(result);
            Assert.Equal("Concurrent Test", result.First().Body);
        });
    }

    [Fact]
    public async Task ConcurrentGetSystemMessage_ThreadSafe()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("Concurrent message");

        // Act - Multiple concurrent calls
        var tasks = Enumerable.Range(0, 15).Select(_ => _newsService.GetSystemMessage()).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        _mockRepo.Verify(r => r.GetSystemMessageAsync(), Times.Once);
        Assert.All(results, result => Assert.Equal("Concurrent message", result));
    }

    [Fact]
    public async Task ConcurrentReinitialize_ThreadSafe()
    {
        // Arrange
        var callCount = 0;
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(() =>
        {
            Interlocked.Increment(ref callCount);
            return new List<NewsEntity>
            {
                new() { Id = callCount, Body = $"Call {callCount}", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
        });
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("test");

        // Act - Multiple concurrent reinitialize calls
        var reinitializeTasks = Enumerable.Range(0, 10).Select(_ => _newsService.Reinitialize()).ToArray();
        await Task.WhenAll(reinitializeTasks);

        var result = await _newsService.GetNews();

        // Assert - Should have been called at least once, result should be consistent
        Assert.True(callCount >= 1);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetNews_AfterReinitialize_UsesNewRepositoryCall()
    {
        // Arrange
        var initialNews = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Initial", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        var newNews = new List<NewsEntity>
        {
            new() { Id = 2, Body = "After Reinitialize", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };

        _mockRepo.SetupSequence(r => r.GetActiveNewsAsync())
            .ReturnsAsync(initialNews)
            .ReturnsAsync(newNews);
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("");

        // Act
        await _newsService.GetNews(); // First initialization
        await _newsService.Reinitialize(); // Force reinitialize
        var result = await _newsService.GetNews(); // Should use new data

        // Assert
        _mockRepo.Verify(r => r.GetActiveNewsAsync(), Times.Exactly(2));
        Assert.Single(result);
        Assert.Equal("After Reinitialize", result.First().Body);
    }

    #endregion
}
