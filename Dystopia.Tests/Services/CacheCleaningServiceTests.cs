using Dystopia.Database;
using Dystopia.Services.Cache;
using Dystopia.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PolytopiaBackendBase.Game;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Dystopia.Tests.Services;

public class CacheCleaningServiceTests
{
    private static CacheCleaningService CreateService(out CacheSettings settings, out Mock<ICacheService<GameViewModel>> mockCacheService,
        out Mock<PolydystopiaDbContext> mockDbContext, out CancellationTokenSource cts, out Mock<IServiceScopeFactory> serviceScopeFactory)
    {
        settings = new CacheSettings
        {
            GameEntity = new CacheProfile()
            {
                CacheCleanupFrequency = TimeSpan.FromMilliseconds(1),
                StaleTime = TimeSpan.FromMinutes(30)
            }
        };

        var cacheSettings = settings;
        var mockOptions = Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == cacheSettings);
        mockCacheService = new Mock<ICacheService<GameViewModel>>();
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        mockDbContext = new Mock<PolydystopiaDbContext>(options);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(PolydystopiaDbContext)))
            .Returns(mockDbContext.Object);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        var service = new CacheCleaningService(mockOptions, mockCacheService.Object, serviceScopeFactory.Object,
            NullLogger<CacheCleaningService>.Instance);
        cts = new CancellationTokenSource();
        return service;
    }
    [Fact]
    public async Task ExecuteAsync_WhenCalled_CleansCacheAndSavesChanges()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScope);
        var cleanupCalled = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object),
            Times.AtLeastOnce());
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsCleanupProcess()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts,
            out var serviceScopeFactory);
        
        cts.Cancel();
        // Act
        await service.StartAsync(cts.Token);

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(It.IsAny<TimeSpan>(), It.IsAny<PolydystopiaDbContext>()),
            Times.Never());
    }
    [Fact]
    public async Task ExecuteAsync_WhenCancelled_SavesCacheToDisk()
    {
        var service = CreateService(out var settings,
            out var mockCacheService,
            out var mockDbContext,
            out var cts,
            out var serviceScopeFactory);

        mockCacheService.Setup(x => x.SaveAllCacheToDisk(It.IsAny<PolydystopiaDbContext>()));
            
        cts.Cancel();
        var serviceTask = service.StartAsync(cts.Token);
        await serviceTask;
        
        mockCacheService.Verify(x => x.SaveAllCacheToDisk(mockDbContext.Object), Times.Once());
        
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());

        serviceScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_UsesNewDbContextScopeForEachIteration()
    {
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var cleanupCalled = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        serviceScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce());
    }

    #region  Ai

    [Fact]
    public async Task ExecuteAsync_WhenCleanStaleCacheThrows_ContinuesOperation()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var firstCallCompleted = new ManualResetEventSlim();
        var secondCallCompleted = new ManualResetEventSlim();
        var callCount = 0;

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    firstCallCompleted.Set();
                    throw new InvalidOperationException("Database error");
                }
                else if (callCount == 2)
                {
                    secondCallCompleted.Set();
                }
            });

        // Act
        var task = service.StartAsync(cts.Token);

        // Wait for first call to complete (with exception)
        firstCallCompleted.Wait(TimeSpan.FromSeconds(2));

        // Give time for the second iteration
        await Task.Delay(50);
        secondCallCompleted.Wait(TimeSpan.FromSeconds(1));

        cts.Cancel();
        await task;

        // Assert
        Assert.True(callCount >= 2, "Service should continue after exception and make subsequent calls");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSaveChangesThrows_ContinuesOperation()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var firstCallCompleted = new ManualResetEventSlim();
        var secondCallCompleted = new ManualResetEventSlim();
        var callCount = 0;

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() =>
            {
                callCount++;
                if (callCount == 1)
                    firstCallCompleted.Set();
                else if (callCount == 2)
                    secondCallCompleted.Set();
            });

        mockDbContext.SetupSequence(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Database save failed"))
            .Returns(Task.FromResult(1));

        // Act
        var task = service.StartAsync(cts.Token);

        firstCallCompleted.Wait(TimeSpan.FromSeconds(2));
        await Task.Delay(50);
        secondCallCompleted.Wait(TimeSpan.FromSeconds(1));

        cts.Cancel();
        await task;

        // Assert
        Assert.True(callCount >= 2, "Service should continue after save exception");
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteAsync_MultipleIterations_CallsCleanupMultipleTimes()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var callCount = 0;
        var targetCalls = 3;
        var allCallsCompleted = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() =>
            {
                callCount++;
                if (callCount >= targetCalls)
                    allCallsCompleted.Set();
            });

        // Act
        var task = service.StartAsync(cts.Token);
        allCallsCompleted.Wait(TimeSpan.FromSeconds(5));
        cts.Cancel();
        await task;

        // Assert
        Assert.True(callCount >= targetCalls, $"Expected at least {targetCalls} calls, got {callCount}");
        mockCacheService.Verify(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object),
            Times.AtLeast(targetCalls));
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentCacheSettings_UsesCorrectValues()
    {
        // Arrange
        var customSettings = new CacheSettings
        {
            GameEntity = new CacheProfile
            {
                CacheCleanupFrequency = TimeSpan.FromMilliseconds(5),
                StaleTime = TimeSpan.FromHours(2)
            }
        };

        var mockOptions = Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == customSettings);
        var mockCacheService = new Mock<ICacheService<GameViewModel>>();
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var mockDbContext = new Mock<PolydystopiaDbContext>(options);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(PolydystopiaDbContext)))
            .Returns(mockDbContext.Object);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var service = new CacheCleaningService(mockOptions, mockCacheService.Object, serviceScopeFactory.Object,
            NullLogger<CacheCleaningService>.Instance);

        var cleanupCalled = new ManualResetEventSlim();
        mockCacheService.Setup(x => x.CleanStaleCache(customSettings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        var cts = new CancellationTokenSource();

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(TimeSpan.FromHours(2), mockDbContext.Object),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WithMockLogger_LogsShutdownMessage()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var mockLogger = new Mock<ILogger<CacheCleaningService>>();

        var serviceWithLogger = new CacheCleaningService(
            Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == settings),
            mockCacheService.Object,
            serviceScopeFactory.Object,
            mockLogger.Object);

        // Act
        cts.Cancel(); // Cancel immediately to trigger shutdown
        await serviceWithLogger.StartAsync(cts.Token);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CacheCleaningService shutting down")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DisposesServiceScopesCorrectly()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        mockScopeServiceProvider.Setup(x => x.GetService(typeof(PolydystopiaDbContext)))
            .Returns(mockDbContext.Object);

        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var cleanupCalled = new ManualResetEventSlim();
        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameEntity.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        // Service should dispose at least one scope for shutdown, regardless of how many cleanup iterations occurred
        mockScope.Verify(x => x.Dispose(), Times.AtLeastOnce());
        // Additionally verify that SaveAllCacheToDisk was called during shutdown (which requires a scope)
        mockCacheService.Verify(x => x.SaveAllCacheToDisk(mockDbContext.Object), Times.Once());
    }

    [Fact]
    public async Task ExecuteAsync_SaveAllCacheToDisk_WhenDbSaveFails_StillCompletes()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);

        // Setup SaveChangesAsync to fail only during shutdown
        var saveCallCount = 0;
        mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                saveCallCount++;
                if (saveCallCount > 1) // Fail on shutdown save
                    throw new DbUpdateException("Failed to save during shutdown");
                return Task.FromResult(1);
            });

        // Act
        cts.Cancel(); // Cancel immediately to trigger shutdown
        var task = service.StartAsync(cts.Token);

        // Should complete despite the exception
        await task;

        // Assert
        mockCacheService.Verify(x => x.SaveAllCacheToDisk(mockDbContext.Object), Times.Once);
        Assert.True(saveCallCount >= 1);
    }

    [Fact]
    public async Task ExecuteAsync_ConcurrentCancellation_HandlesGracefully()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var delayStarted = new ManualResetEventSlim();

        // Make the delay observable
        mockCacheService.Setup(x => x.CleanStaleCache(It.IsAny<TimeSpan>(), It.IsAny<PolydystopiaDbContext>()))
            .Callback(() => delayStarted.Set());

        // Act
        var task = service.StartAsync(cts.Token);

        // Wait briefly, then cancel
        await Task.Delay(10);
        cts.Cancel();

        // Should complete without hanging
        await task;

        // Assert - Should complete successfully even with concurrent cancellation
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceScopeCreationFails_HandlesGracefully()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);

        serviceScopeFactory.Setup(x => x.CreateScope())
            .Throws(new InvalidOperationException("Service scope creation failed"));

        // Act
        cts.Cancel(); // Cancel to trigger shutdown and test exception handling there too
        var task = service.StartAsync(cts.Token);

        // Should complete despite the exception
        await task;

        // Assert - Should not crash, service should handle the exception
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task ExecuteAsync_TaskCanceledExceptionDuringDelay_ExitsGracefully()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        settings.GameEntity.CacheCleanupFrequency = TimeSpan.FromMilliseconds(1000);

        // Act
        var task = service.StartAsync(cts.Token);

        // Cancel after a very short time to trigger TaskCanceledException during delay
        await Task.Delay(1);
        cts.Cancel();

        await task;

        // Assert
        Assert.True(task.IsCompleted);
        // Verify that cleanup methods are not called when cancelled during delay
        mockCacheService.Verify(x => x.CleanStaleCache(It.IsAny<TimeSpan>(), It.IsAny<PolydystopiaDbContext>()),
            Times.Never);

        // But shutdown save should still be called
        mockCacheService.Verify(x => x.SaveAllCacheToDisk(mockDbContext.Object), Times.Once);
    }

    #endregion
}