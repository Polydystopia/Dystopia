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

namespace Dystopia.Tests.Services;

public class CacheCleaningServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCalled_CleansCacheAndSavesChanges()
    {
        // Arrange
        var settings = new CacheSettings
        {
            GameViewModel = new CacheProfile()
            {
                CacheCleanupFrequency = TimeSpan.FromMilliseconds(1),
                StaleTime = TimeSpan.FromMinutes(30)
            }
        };

        var mockOptions = Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == settings);
        var mockCacheService = new Mock<ICacheService<GameViewModel>>();
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var mockDbContext = new Mock<PolydystopiaDbContext>(options);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(PolydystopiaDbContext)))
            .Returns(mockDbContext.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        var service = new CacheCleaningService(mockOptions, mockCacheService.Object, serviceProvider.Object,
            NullLogger<CacheCleaningService>.Instance);
        var cts = new CancellationTokenSource();
        var cleanupCalled = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameViewModel.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(settings.GameViewModel.StaleTime, mockDbContext.Object),
            Times.AtLeastOnce());
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsCleanupProcess()
    {
        // Arrange
        var settings = new CacheSettings
        {
            GameViewModel = new CacheProfile()
            {
                CacheCleanupFrequency = TimeSpan.FromMinutes(5),
                StaleTime = TimeSpan.FromMinutes(30)
            }
        };

        var mockOptions = Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == settings);
        var mockCacheService = new Mock<ICacheService<GameViewModel>>();
        var serviceProvider = new Mock<IServiceProvider>();

        var service = new CacheCleaningService(mockOptions, mockCacheService.Object, serviceProvider.Object, NullLogger<CacheCleaningService>.Instance);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(It.IsAny<TimeSpan>(), It.IsAny<PolydystopiaDbContext>()),
            Times.Never());
    }

    [Fact]
    public async Task ExecuteAsync_UsesNewDbContextScopeForEachIteration()
    {
        // Arrange
        var settings = new CacheSettings
        {
            GameViewModel = new CacheProfile()
            {

                CacheCleanupFrequency = TimeSpan.FromMilliseconds(1),
                StaleTime = TimeSpan.FromMinutes(30)
            }
        };

        var mockOptions = Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == settings);
        var mockCacheService = new Mock<ICacheService<GameViewModel>>();
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var mockDbContext = new Mock<PolydystopiaDbContext>(options);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(PolydystopiaDbContext)))
            .Returns(mockDbContext.Object);
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        var service = new CacheCleaningService(mockOptions, mockCacheService.Object, serviceProvider.Object, NullLogger<CacheCleaningService>.Instance);;
        var cts = new CancellationTokenSource();
        var cleanupCalled = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameViewModel.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        serviceScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce());
    }
}