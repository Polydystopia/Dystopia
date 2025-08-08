using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dystopia.Tests.Services;

using Dystopia.Services.Cache;
using Dystopia.Database;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class CacheServiceTests
{
    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueAndValue()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var key = Guid.NewGuid();
        var expectedValue = "test value";
        service.Set(key, expectedValue, _ => { });

        // Act
        var result = service.TryGet(key, out var actualValue);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        
        // Act
        var result = service.TryGet(Guid.NewGuid(), out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Set_AddsItemToCache()
    {
        // Arrange
        var service = new CacheService<int>(NullLogger<CacheService<int>>.Instance);;
        var key = Guid.NewGuid();
        
        // Act
        service.Set(key, 42, _ => { });
        
        // Assert
        Assert.True(service.TryGet(key, out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryRemove_RemovesExistingItem()
    {
        // Arrange
        var service = new CacheService<bool>(NullLogger<CacheService<bool>>.Instance);
        var key = Guid.NewGuid();
        service.Set(key, true, _ => { });
        
        // Act
        service.TryRemove(key);
        
        // Assert
        Assert.False(service.TryGet(key, out _));
    }

    [Fact]
    public void CleanStaleCache_RemovesAndSavesStaleItems()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);
        var staleKey = Guid.NewGuid();
        var freshKey = Guid.NewGuid();

        // Set up items with lastUsed times
        service.Set(staleKey, "stale", _ => { });
        service.Set(freshKey, "fresh", _ => { });

        // Manipulate lastUsed times using reflection
        var cacheField = typeof(CacheService<string>).GetField("_cache", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cache = (System.Collections.Concurrent.ConcurrentDictionary<Guid, (string value, DateTime lastUsed, Action<PolydystopiaDbContext> saveToDisk)>)
            cacheField.GetValue(service)!;

        // Make staleKey item 2 hours old
        cache.TryUpdate(staleKey, 
            (cache[staleKey].value, DateTime.Now.AddHours(-2), cache[staleKey].saveToDisk), 
            cache[staleKey]);

        // Act
        service.CleanStaleCache(TimeSpan.FromHours(1), dbContextMock.Object);

        // Assert
        Assert.False(service.TryGet(staleKey, out _));
        Assert.True(service.TryGet(freshKey, out _));
    }

    [Fact]
    public void CleanStaleCache_CallsSaveToDiskForStaleItems()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);
        var key = Guid.NewGuid();
        var saveCalled = false;

        service.Set(key, "test", _ => saveCalled = true);
        
        // Make item stale
        var cacheField = typeof(CacheService<string>).GetField("_cache", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cache = (System.Collections.Concurrent.ConcurrentDictionary<Guid, (string value, DateTime lastUsed, Action<PolydystopiaDbContext> saveToDisk)>)
            cacheField.GetValue(service)!;
        
        cache.TryUpdate(key, 
            (cache[key].value, DateTime.Now.AddHours(-2), cache[key].saveToDisk), 
            cache[key]);

        // Act
        service.CleanStaleCache(TimeSpan.FromHours(1), dbContextMock.Object);
        // Assert
        Assert.True(saveCalled);
    }

    #region AI

    [Fact]
    public void TryGetAll_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.TryGetAll(null!, out _));
    }

    [Fact]
    public void TryGetAll_WithMatchingPredicate_ReturnsMatchingItems()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var key3 = Guid.NewGuid();

        service.Set(key1, "apple", _ => { });
        service.Set(key2, "banana", _ => { });
        service.Set(key3, "apricot", _ => { });

        // Act
        service.TryGetAll(x => x.StartsWith("a"), out var values);

        // Assert
        Assert.Equal(2, values.Count);
        Assert.Contains("apple", values);
        Assert.Contains("apricot", values);
        Assert.DoesNotContain("banana", values);
    }

    [Fact]
    public void TryGetAll_WithNonMatchingPredicate_ReturnsEmptyList()
    {
        // Arrange
        var service = new CacheService<int>(NullLogger<CacheService<int>>.Instance);
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();

        service.Set(key1, 5, _ => { });
        service.Set(key2, 10, _ => { });

        // Act
        service.TryGetAll(x => x > 20, out var values);

        // Assert
        Assert.Empty(values);
    }

    [Fact]
    public void TryGetAll_WithEmptyCache_ReturnsEmptyList()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);

        // Act
        service.TryGetAll(x => true, out var values);

        // Assert
        Assert.Empty(values);
    }

    [Fact]
    public void SaveAllCacheToDisk_CallsSaveToDiskForAllItems()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);

        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var save1Called = false;
        var save2Called = false;

        service.Set(key1, "value1", _ => save1Called = true);
        service.Set(key2, "value2", _ => save2Called = true);

        // Act
        service.SaveAllCacheToDisk(dbContextMock.Object);

        // Assert
        Assert.True(save1Called);
        Assert.True(save2Called);
    }

    [Fact]
    public void SaveAllCacheToDisk_WithEmptyCache_DoesNotThrow()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);

        // Act & Assert (should not throw)
        service.SaveAllCacheToDisk(dbContextMock.Object);
    }

    [Fact]
    public void TryGet_UpdatesLastUsedTime()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var key = Guid.NewGuid();
        var value = "test";

        service.Set(key, value, _ => { });

        // Get the initial lastUsed time using reflection
        var cacheField = typeof(CacheService<string>).GetField("_cache",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cache = (System.Collections.Concurrent.ConcurrentDictionary<Guid, (string value, DateTime lastUsed, Action<PolydystopiaDbContext> saveToDisk)>)
            cacheField.GetValue(service)!;

        var initialTime = cache[key].lastUsed;

        // Wait a small amount to ensure time difference
        Thread.Sleep(5);

        // Act
        service.TryGet(key, out _);

        // Assert
        var updatedTime = cache[key].lastUsed;
        Assert.True(updatedTime > initialTime);
    }

    [Fact]
    public void Set_OverwritesExistingValue()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var key = Guid.NewGuid();
        var originalValue = "original";
        var newValue = "new";

        service.Set(key, originalValue, _ => { });

        // Act
        service.Set(key, newValue, _ => { });

        // Assert
        Assert.True(service.TryGet(key, out var retrievedValue));
        Assert.Equal(newValue, retrievedValue);
    }

    [Fact]
    public void TryRemove_NonExistentKey_DoesNotThrow()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var nonExistentKey = Guid.NewGuid();

        // Act & Assert (should not throw)
        service.TryRemove(nonExistentKey);
    }

    [Fact]
    public void CleanStaleCache_WithNoStaleItems_DoesNotCallSaveToDisk()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);

        var key = Guid.NewGuid();
        var saveCalled = false;

        service.Set(key, "fresh", _ => saveCalled = true);

        // Act
        service.CleanStaleCache(TimeSpan.FromHours(1), dbContextMock.Object);

        // Assert
        Assert.False(saveCalled);
        Assert.True(service.TryGet(key, out _));
    }

    [Fact]
    public void CleanStaleCache_WithZeroStaleTime_RemovesAllItems()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);

        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var save1Called = false;
        var save2Called = false;

        service.Set(key1, "value1", _ => save1Called = true);
        service.Set(key2, "value2", _ => save2Called = true);

        // Act
        service.CleanStaleCache(TimeSpan.Zero, dbContextMock.Object);

        // Assert
        Assert.True(save1Called);
        Assert.True(save2Called);
        Assert.False(service.TryGet(key1, out _));
        Assert.False(service.TryGet(key2, out _));
    }

    [Fact]
    public async Task ConcurrentSetAndGet_ThreadSafe()
    {
        // Arrange
        var service = new CacheService<int>(NullLogger<CacheService<int>>.Instance);
        var key = Guid.NewGuid();
        var taskCount = 100;
        var tasks = new Task[taskCount];
        var results = new bool[taskCount];

        // Act - Multiple threads setting and getting concurrently
        for (int i = 0; i < taskCount; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                service.Set(key, index, _ => { });
                results[index] = service.TryGet(key, out _);
            });
        }

        await Task.WhenAll(tasks);

        // Assert - All operations should complete successfully
        Assert.All(results, result => Assert.True(result));
        Assert.True(service.TryGet(key, out var finalValue));
        Assert.True(finalValue >= 0 && finalValue < taskCount);
    }

    [Fact]
    public async Task ConcurrentTryGetAll_ThreadSafe()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var keyCount = 50;

        // Pre-populate cache
        for (int i = 0; i < keyCount; i++)
        {
            service.Set(Guid.NewGuid(), $"value{i}", _ => { });
        }

        var taskCount = 20;
        var tasks = new Task<IList<string>>[taskCount];

        // Act - Multiple threads calling TryGetAll concurrently
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                service.TryGetAll(x => x.StartsWith("value"), out var values);
                return values;
            });
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be consistent
        Assert.All(results, result => Assert.Equal(keyCount, result.Count));
    }

    [Fact]
    public async Task ConcurrentSetAndRemove_ThreadSafe()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var keys = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
        var taskCount = keys.Length;
        var tasks = new Task[taskCount * 2]; // Set and Remove tasks

        // Act - Concurrent Set and Remove operations
        for (int i = 0; i < taskCount; i++)
        {
            var index = i;
            var key = keys[index];

            tasks[i] = Task.Run(() => service.Set(key, $"value{index}", _ => { }));
            tasks[i + taskCount] = Task.Run(() => service.TryRemove(key));
        }

        await Task.WhenAll(tasks);

        // Assert - Operations complete without exceptions
        // Final state is non-deterministic due to concurrency, but no exceptions should occur
        Assert.True(true); // Test passes if no exceptions were thrown
    }

    [Fact]
    public async Task ConcurrentCleanStaleCache_ThreadSafe()
    {
        // Arrange
        var service = new CacheService<string>(NullLogger<CacheService<string>>.Instance);
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);

        // Pre-populate with items
        for (int i = 0; i < 50; i++)
        {
            service.Set(Guid.NewGuid(), $"value{i}", _ => { });
        }

        var taskCount = 10;
        var tasks = new Task[taskCount];

        // Act - Multiple threads calling CleanStaleCache concurrently
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
                service.CleanStaleCache(TimeSpan.FromMinutes(1), dbContextMock.Object));
        }

        // Assert - Should complete without exceptions
        await Task.WhenAll(tasks);
        Assert.True(true); // Test passes if no exceptions were thrown
    }

    #endregion
}
