using System.Net;
using Microsoft.EntityFrameworkCore;
using ReviewsService.Application.Features.Reviews.HasReviewedProduct;
using ReviewsService.Domain.Entities;
using ReviewsService.Infrastructure.Persistence;

namespace OLXKiller.Tests.ReviewsServiceTests;

public class HasReviewedProductQueryHandlerTests
{
    private readonly DbContextOptions<ReviewsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<ReviewsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Guid _anotherUserId = Guid.NewGuid();
    private readonly Guid _anotherProductId = Guid.NewGuid();

    private ReviewsDbContext CreateDbContext()
    {
        return new ReviewsDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Handler_ReturnsTrue_When_UserHasReviewedProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = "Great product",
            Rate = 5
        });
        
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.True(result.Data); 
    }

    [Fact]
    public async Task Handler_ReturnsFalse_When_UserHasNotReviewedProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.False(result.Data); 
    }

    [Fact]
    public async Task Handler_ReturnsFalse_When_OtherUserReviewedProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _anotherUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _anotherUserId,
            ProductId = _testProductId,
            Text = "Good product",
            Rate = 4
        });
        
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.False(result.Data); 
    }

    [Fact]
    public async Task Handler_ReturnsFalse_When_UserReviewedOtherProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _anotherProductId });
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _testUserId,
            ProductId = _anotherProductId,
            Text = "Not this product",
            Rate = 3
        });
        
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.False(result.Data); 
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Error);
        Assert.False(result.Data); 
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ProductDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("Product was not found", result.Error);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_BothUserAndProductDoNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Error);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task Handler_ReturnsTrue_ForMultipleReviewsBySameUser()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = productId1 });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = productId2 });
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _testUserId,
            ProductId = productId1,
            Text = "Product 1 review",
            Rate = 5
        });
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _testUserId,
            ProductId = productId2,
            Text = "Product 2 review",
            Rate = 4
        });
        
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);

        // Act 
        var result1 = await handler.Handle(
            new HasReviewedProductQuery(_testUserId, productId1), 
            CancellationToken.None);

        // Act 
        var result2 = await handler.Handle(
            new HasReviewedProductQuery(_testUserId, productId2), 
            CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result1.Data); 
        
        Assert.True(result2.IsSuccess);
        Assert.True(result2.Data); 
    }

    [Fact]
    public async Task Handler_WorksWithDeletedReviews()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        
        var review = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = "Deleted review",
            Rate = 3
        };
        
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task Handler_UsesAsNoTracking_ForSnapshotQueries()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var user = new UserSnapshot { Id = _testUserId };
        var product = new ProductSnapshot { Id = _testProductId };
        
        dbContext.UserSnapshots.Add(user);
        dbContext.ProductSnapshots.Add(product);
        await dbContext.SaveChangesAsync();
        
        user.Id = Guid.NewGuid(); 
        product.Id = Guid.NewGuid();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task Handler_ReturnsCorrectResult_ForEmptyDatabase()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Error);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task Handler_HandlesMultipleCallsCorrectly()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act 
        var result1 = await handler.Handle(query, CancellationToken.None);
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = "Just added",
            Rate = 5
        });
        await dbContext.SaveChangesAsync();
        
        // Act 
        var result2 = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.False(result1.Data); 
        
        Assert.True(result2.IsSuccess);
        Assert.True(result2.Data); 
    }

    [Fact]
    public async Task Handler_WorksWithNullDatabaseContext()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
    }

    [Fact]
    public async Task Handler_ReturnsTrue_ForReviewWithMinimalData()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        
        dbContext.Reviews.Add(new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = "", 
            Rate = 1   
        });
        
        await dbContext.SaveChangesAsync();

        var handler = new HasReviewedProductQueryHandler(dbContext);
        var query = new HasReviewedProductQuery(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data); 
    }
}