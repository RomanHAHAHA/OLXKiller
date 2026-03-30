using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.Review;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using ReviewsService.Application.Features.Reviews.SetStatus;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Enums;
using ReviewsService.Domain.Interfaces;

namespace OLXKiller.Tests.ReviewsServiceTests;

public class SetReviewStatusCommandHandlerTests
{
    private readonly Mock<IReviewsRepository> _repositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IHttpUserContext> _httpContextMock = new();

    private readonly SetReviewStatusCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Guid _testAdminUserId = Guid.NewGuid();
    private readonly ReviewStatus _newStatus = ReviewStatus.Approved;
    private readonly ReviewStatus _initialStatus = ReviewStatus.Pending;

    public SetReviewStatusCommandHandlerTests()
    {
        var serviceOptions = Options.Create(new ServiceOptions { Name = "ReviewsService" });
        
        _httpContextMock
            .Setup(x => x.UserId)
            .Returns(_testAdminUserId);

        _handler = new SetReviewStatusCommandHandler(
            _repositoryMock.Object,
            _publisherMock.Object,
            _httpContextMock.Object,
            serviceOptions);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ReviewStatusUpdated()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(
            _testUserId, _testProductId, It.IsAny<CancellationToken>()), Times.Once);
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.Equal(_newStatus, existingReview.Status);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ReviewDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("Review was not found", result.Description);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(
            _testUserId, _testProductId, It.IsAny<CancellationToken>()), Times.Once);
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(p => p.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(p => p.Publish(It.IsAny<ReviewStatusUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(ReviewStatus.Pending, ReviewStatus.Approved)]
    [InlineData(ReviewStatus.Approved, ReviewStatus.Rejected)]
    [InlineData(ReviewStatus.Pending, ReviewStatus.Rejected)]
    [InlineData(ReviewStatus.Rejected, ReviewStatus.Approved)]
    public async Task Handler_UpdatesAllStatusTransitions(ReviewStatus fromStatus, ReviewStatus toStatus)
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = fromStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, toStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(toStatus, existingReview.Status);
    }

    [Fact]
    public async Task Handler_PublishesCorrectSystemActionEvent()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<SystemActionEvent>(e => 
                e.SenderServiceName == "ReviewsService" &&
                e.UserId == _testAdminUserId &&
                e.ActionType == ActionType.Update &&
                e.Message.Contains(_testUserId.ToString()) &&
                e.Message.Contains(_testProductId.ToString()) &&
                e.Message.Contains(_newStatus.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesCorrectReviewStatusUpdatedEvent()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<ReviewStatusUpdatedEvent>(e => 
                e.SenderServiceName == "ReviewsService" &&
                e.ProductId == _testProductId &&
                e.CorrelationId != Guid.Empty),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesSameCorrelationIdForBothEvents()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        Guid? capturedCorrelationId = null;
        _publisherMock.Setup(p => p.Publish(
                It.IsAny<SystemActionEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<SystemActionEvent, CancellationToken>((e, _) => capturedCorrelationId = e.CorrelationId);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<ReviewStatusUpdatedEvent>(e => 
                e.CorrelationId == capturedCorrelationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesHttpContextUserId_ForSystemActionEvent()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        _httpContextMock
            .Setup(x => x.UserId)
            .Returns(adminUserId);

        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == adminUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotPublishEvents_When_ReviewNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<SystemActionEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
        
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<ReviewStatusUpdatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DoesNotSaveChanges_When_EventPublishingFails()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        _publisherMock
            .Setup(p => p.Publish(
                It.IsAny<SystemActionEvent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publishing failed"));
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        
        Assert.Equal(_newStatus, existingReview.Status);
    }

    [Fact]
    public async Task Handler_WorksWithDifferentServiceNames()
    {
        // Arrange
        var customServiceOptions = Options.Create(new ServiceOptions { Name = "CustomService" });
        var handler = new SetReviewStatusCommandHandler(
            _repositoryMock.Object,
            _publisherMock.Object,
            _httpContextMock.Object,
            customServiceOptions);

        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<SystemActionEvent>(e => 
                e.SenderServiceName == "CustomService"),
            It.IsAny<CancellationToken>()), Times.Once);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<ReviewStatusUpdatedEvent>(e => 
                e.SenderServiceName == "CustomService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);
        
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Status = _initialStatus,
            Text = "Test review",
            Rate = 4
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_testUserId, _testProductId, cancellationToken))
            .ReturnsAsync(existingReview);
        
        var command = new SetReviewStatusCommand(_testUserId, _testProductId, _newStatus);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(_testUserId, _testProductId, cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(cancellationToken), Times.Once);
        
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<SystemActionEvent>(),
            cancellationToken), Times.Once);
        
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<ReviewStatusUpdatedEvent>(),
            cancellationToken), Times.Once);
    }
}