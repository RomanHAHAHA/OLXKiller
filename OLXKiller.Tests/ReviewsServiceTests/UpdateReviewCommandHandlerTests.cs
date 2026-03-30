using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using ReviewsService.Application.Features.Reviews.Create;
using ReviewsService.Application.Features.Reviews.Update;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Interfaces;

namespace OLXKiller.Tests.ReviewsServiceTests;

public class UpdateReviewCommandHandlerTests
{
    private readonly Mock<IReviewsRepository> _repositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();

    private readonly UpdateReviewCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private const string OldText = "Old review text";
    private const int OldRate = 3;
    private const string NewText = "Updated review text";
    private const int NewRate = 5;

    public UpdateReviewCommandHandlerTests()
    {
        var serviceOptions = Options.Create(new ServiceOptions { Name = "ReviewsService" });
        
        _handler = new UpdateReviewCommandHandler(
            _repositoryMock.Object,
            _publisherMock.Object,
            serviceOptions);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ReviewUpdated()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = OldText,
            Rate = OldRate
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(
            _testUserId, _testProductId, It.IsAny<CancellationToken>()), Times.Once);
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.Equal(NewText, existingReview.Text);
        Assert.Equal(NewRate, existingReview.Rate);
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
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

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
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveFails()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = OldText,
            Rate = OldRate
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.Equal(NewText, existingReview.Text);
        Assert.Equal(NewRate, existingReview.Rate);
    }

    [Fact]
    public async Task Handler_PublishesCorrectSystemActionEvent()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = OldText,
            Rate = OldRate
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.Is<SystemActionEvent>(e => 
                e.SenderServiceName == "ReviewsService" &&
                e.UserId == _testUserId &&
                e.ActionType == ActionType.Update &&
                e.Message.Contains(_testUserId.ToString()) &&
                e.Message.Contains(_testProductId.ToString()) &&
                e.Message.Contains("updated")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Short text", "Very long updated text with more details about the product")]
    [InlineData("Long original text", "Short")]
    [InlineData("Same length", "Diff content")]
    [InlineData("", "Now has text")]
    [InlineData("Had text", "")]
    public async Task Handler_UpdatesTextCorrectly(string originalText, string newText)
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = originalText,
            Rate = OldRate
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, newText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newText, existingReview.Text);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 1)]
    [InlineData(3, 3)]
    [InlineData(2, 4)]
    [InlineData(4, 2)]
    public async Task Handler_UpdatesRateCorrectly(int originalRate, int newRate)
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = OldText,
            Rate = originalRate
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, newRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newRate, existingReview.Rate);
    }

    [Fact]
    public async Task Handler_DoesNotPublishEvent_When_ReviewNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<SystemActionEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PublishesEventBeforeSaving()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = OldText,
            Rate = OldRate
        };

        var publishCalled = false;
        var saveChangesCalled = false;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => publishCalled = true)
            .Returns(Task.CompletedTask);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveChangesCalled = true)
            .ReturnsAsync(true);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(publishCalled);
        Assert.True(saveChangesCalled);
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
            Text = OldText,
            Rate = OldRate
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_testUserId, _testProductId, cancellationToken))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(_testUserId, _testProductId, cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(cancellationToken), Times.Once);
        
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<SystemActionEvent>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotSaveChanges_When_EventPublishingFails()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = OldText,
            Rate = OldRate
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
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, NewText, NewRate),
            _testUserId);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        
        Assert.Equal(NewText, existingReview.Text);
        Assert.Equal(NewRate, existingReview.Rate);
    }

    [Fact]
    public async Task Handler_WorksWithMinimalReviewData()
    {
        // Arrange
        var existingReview = new Review
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Text = "", 
            Rate = 1   
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                _testProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var updatedText = "Now has content";
        var updatedRate = 5;
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(_testProductId, updatedText, updatedRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updatedText, existingReview.Text);
        Assert.Equal(updatedRate, existingReview.Rate);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_LookingForWrongProductReview()
    {
        // Arrange
        var wrongProductId = Guid.NewGuid();
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                _testUserId, 
                wrongProductId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);
        
        var command = new UpdateReviewCommand(
            new ReviewCreateDto(wrongProductId, NewText, NewRate),
            _testUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        
        _repositoryMock.Verify(r => r.GetByIdAsync(
            _testUserId, wrongProductId, It.IsAny<CancellationToken>()), Times.Once);
    }
}