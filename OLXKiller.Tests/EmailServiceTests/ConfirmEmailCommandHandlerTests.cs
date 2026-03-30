using System.Net;
using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.Email;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using EmailService.Application.Features.EmailConfirmations.ConfirmEmail;

namespace OLXKiller.Tests.EmailServiceTests;

public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<ICacheService<string>> _cacheServiceMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly ConfirmEmailCommandHandler _handler;
    
    private const string TestEmail = "test@example.com";
    private const string TestCode = "123456";
    private const string HashedCode = "hashed_123456";
    
    public ConfirmEmailCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(EmailService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _handler = new ConfirmEmailCommandHandler(
            _cacheServiceMock.Object,
            _passwordHasherMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_CodeIsValid()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify(TestCode, HashedCode))
            .Returns(true);
            
        _cacheServiceMock
            .Setup(x => x.RemoveAsync(TestEmail, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _cacheServiceMock.Verify(x => x.GetAsync(TestEmail, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(TestCode, HashedCode), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync(TestEmail, CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<EmailConfirmedEvent>(e => 
                e.Email == TestEmail &&
                e.SenderServiceName == "EmailService"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_CodeIsExpired()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Status);
        Assert.Equal("Confirmation time was expired", result.Description);
        
        _cacheServiceMock.Verify(x => x.GetAsync(TestEmail, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_CodeIsInvalid()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, "wrong_code");
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify("wrong_code", HashedCode))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Invalid code", result.Description);
        
        _cacheServiceMock.Verify(x => x.GetAsync(TestEmail, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify("wrong_code", HashedCode), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_RemovesCodeFromCache_When_CodeIsValid()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify(TestCode, HashedCode))
            .Returns(true);
            
        bool codeRemoved = false;
        _cacheServiceMock
            .Setup(x => x.RemoveAsync(TestEmail, It.IsAny<CancellationToken>()))
            .Callback(() => codeRemoved = true)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(codeRemoved);
    }

    [Fact]
    public async Task Handler_PublishesEvent_After_SuccessfulConfirmation()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        var correlationIds = new List<Guid>();
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify(TestCode, HashedCode))
            .Returns(true);
            
        _cacheServiceMock
            .Setup(x => x.RemoveAsync(TestEmail, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<EmailConfirmedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((EmailConfirmedEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(correlationIds);
        Assert.NotEqual(Guid.Empty, correlationIds[0]);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_CacheGetFails()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Cache error", exception.Message);
        
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DoesNotRemoveCode_When_VerificationFails()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, "wrong_code");
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify("wrong_code", HashedCode))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        var cancellationToken = new CancellationToken(true);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, cancellationToken))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify(TestCode, HashedCode))
            .Returns(true);
            
        _cacheServiceMock
            .Setup(x => x.RemoveAsync(TestEmail, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _cacheServiceMock.Verify(x => x.GetAsync(TestEmail, cancellationToken), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(TestCode, HashedCode), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync(TestEmail, cancellationToken), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.IsAny<EmailConfirmedEvent>(), 
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_HandlesEmptyCode()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, "");
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify("", HashedCode))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Invalid code", result.Description);
    }

    [Fact]
    public async Task Handler_HandlesNullCode()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, null!);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify(null!, HashedCode))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Invalid code", result.Description);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_EvenIfRemoveCacheFails_ButContinues()
    {
        // Arrange
        var command = new ConfirmEmailCommand(TestEmail, TestCode);
        
        _cacheServiceMock
            .Setup(x => x.GetAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HashedCode);
            
        _passwordHasherMock
            .Setup(x => x.Verify(TestCode, HashedCode))
            .Returns(true);
            
        _cacheServiceMock
            .Setup(x => x.RemoveAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache remove failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Cache remove failed", exception.Message);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}