using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using ReviewsService.Application.Features.Reviews.Create;
using ReviewsService.Domain.Entities;
using ReviewsService.Infrastructure.Persistence;

namespace OLXKiller.Tests.ReviewsServiceTests;

public class CreateReviewCommandHandlerTests
{
    private readonly DbContextOptions<ReviewsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<ReviewsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Guid _anotherUserId = Guid.NewGuid();
    private const string TestReviewText = "Great product!";
    private const int TestRate = 5;

    private ReviewsDbContext CreateDbContext() => new(_dbContextOptions);

    private static Mock<IPublishEndpoint> CreateMockPublisher() => new();

    private static IOptions<ServiceOptions> CreateServiceOptions() 
        => Options.Create(new ServiceOptions { Name = "ReviewsService" });

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ReviewCreated()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, TestReviewText, TestRate),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        var review = await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.UserId == _testUserId && r.ProductId == _testProductId);
        
        Assert.NotNull(review);
        Assert.Equal(TestReviewText, review.Text);
        Assert.Equal(TestRate, review.Rate);
        
        // Verify system action event was published
        publisherMock.Verify(p => p.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId && 
                e.ActionType == ActionType.Create),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        // Only add product snapshot, user snapshot is missing
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, TestReviewText, TestRate),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("UserSnapshot was not found", result.Description);
        
        var reviewsCount = await dbContext.Reviews.CountAsync();
        Assert.Equal(0, reviewsCount);
        
        publisherMock.Verify(p => p.Publish(It.IsAny<SystemActionEvent>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ProductDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        // Only add user snapshot, product snapshot is missing
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, TestReviewText, TestRate),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("ProductSnapshot was not found", result.Description);
        
        var reviewsCount = await dbContext.Reviews.CountAsync();
        Assert.Equal(0, reviewsCount);
        
        publisherMock.Verify(p => p.Publish(It.IsAny<SystemActionEvent>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_CreatesReviewWithCorrectData()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var reviewText = "Excellent quality, fast delivery!";
        var rate = 4;
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, reviewText, rate),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var review = await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.UserId == _testUserId && r.ProductId == _testProductId);
        
        Assert.NotNull(review);
        Assert.Equal(reviewText, review.Text);
        Assert.Equal(rate, review.Rate);
        Assert.NotEqual(default, review.CreatedAt);
    }

    [Fact]
    public async Task Handler_AllowsMultipleReviewsOnSameProductByDifferentUsers()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _anotherUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        
        // Act 
        var result1 = await handler.Handle(
            new CreateReviewCommand(new ReviewCreateDto(_testProductId, "Review 1", 5), _testUserId), 
            CancellationToken.None);
        
        var result2 = await handler.Handle(
            new CreateReviewCommand(new ReviewCreateDto(_testProductId, "Review 2", 4), _anotherUserId), 
            CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        var reviews = await dbContext.Reviews
            .Where(r => r.ProductId == _testProductId)
            .ToListAsync();
        
        Assert.Equal(2, reviews.Count);
        Assert.Contains(reviews, r => r.UserId == _testUserId);
        Assert.Contains(reviews, r => r.UserId == _anotherUserId);
    }

    [Fact]
    public async Task Handler_AllowsSameUserToReviewDifferentProducts()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = productId1 });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = productId2 });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        
        // Act 
        var result1 = await handler.Handle(
            new CreateReviewCommand(new ReviewCreateDto(productId1, "Product 1 review", 5), _testUserId), 
            CancellationToken.None);
        
        var result2 = await handler.Handle(
            new CreateReviewCommand(new ReviewCreateDto(productId2, "Product 2 review", 3), _testUserId), 
            CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        var reviews = await dbContext.Reviews
            .Where(r => r.UserId == _testUserId)
            .ToListAsync();
        
        Assert.Equal(2, reviews.Count);
        Assert.Contains(reviews, r => r.ProductId == productId1);
        Assert.Contains(reviews, r => r.ProductId == productId2);
    }

    [Fact]
    public async Task Handler_PublishesCorrectSystemActionEvent()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, TestReviewText, TestRate),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        publisherMock.Verify(p => p.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId &&
                e.SenderServiceName == "ReviewsService" &&
                e.ActionType == ActionType.Create &&
                e.Message.Contains(_testUserId.ToString()) &&
                e.Message.Contains(_testProductId.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Handler_AcceptsDifferentRatings(int rate)
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, $"Rating {rate}", rate),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var review = await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.UserId == _testUserId && r.ProductId == _testProductId);
        
        Assert.NotNull(review);
        Assert.Equal(rate, review.Rate);
    }

    [Fact]
    public async Task Handler_WorksWithEmptyReviewText()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, "", 4),
            _testUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var review = await dbContext.Reviews
            .FirstOrDefaultAsync(r => r.UserId == _testUserId && r.ProductId == _testProductId);
        
        Assert.NotNull(review);
        Assert.Equal("", review.Text);
    }

    [Fact]
    public async Task Handler_DoesNotSave_WhenEventPublishingFails()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var publisherMock = CreateMockPublisher();
        var serviceOptions = CreateServiceOptions();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testUserId });
        dbContext.ProductSnapshots.Add(new ProductSnapshot { Id = _testProductId });
        await dbContext.SaveChangesAsync();

        publisherMock.Setup(p => p.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publishing failed"));

        var handler = new CreateReviewCommandHandler(dbContext, publisherMock.Object, serviceOptions);
        var command = new CreateReviewCommand(
            new ReviewCreateDto(_testProductId, TestReviewText, TestRate),
            _testUserId);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        
        var reviewsCount = await dbContext.Reviews.CountAsync();
        Assert.Equal(0, reviewsCount);
    }
}