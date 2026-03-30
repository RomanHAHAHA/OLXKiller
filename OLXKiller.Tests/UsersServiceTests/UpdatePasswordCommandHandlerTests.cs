using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using UsersService.Application.Features.Accounts.UpdatePassword;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class UpdatePasswordCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly UpdatePasswordCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private const string OldPassword = "old_password";
    private const string NewPassword = "new_password";
    private const string ConfirmNewPassword = "new_password";
    private const string HashedPassword = "hashed_old_password";
    private const string NewHashedPassword = "hashed_new_password";
    
    public UpdatePasswordCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(UsersService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _handler = new UpdatePasswordCommandHandler(
            _usersRepositoryMock.Object,
            _publisherMock.Object,
            _passwordHasherMock.Object,
            _serviceOptionsMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_PasswordUpdatedSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
        
        var updatePasswordDto = new UpdatePasswordDto(OldPassword, NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock
            .Setup(x => x.Verify(OldPassword, HashedPassword))
            .Returns(true);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(NewPassword))
            .Returns(NewHashedPassword);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.Equal(NewHashedPassword, user.PasswordHash);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(OldPassword, HashedPassword), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(NewPassword), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId &&
                e.ActionType == ActionType.Update &&
                e.SenderServiceName == "UsersService" &&
                e.Message == "Password reset"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var updatePasswordDto = new UpdatePasswordDto(OldPassword, NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Description);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_OldPasswordIncorrect()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
        
        var updatePasswordDto = new UpdatePasswordDto("wrong_old_password", NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock
            .Setup(x => x.Verify("wrong_old_password", HashedPassword))
            .Returns(false);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("Incorrect old password", result.Description);
        Assert.Equal(HashedPassword, user.PasswordHash); 
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify("wrong_old_password", HashedPassword), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId &&
                e.ActionType == ActionType.IncorrectPasswordAttempt &&
                e.SenderServiceName == "UsersService" &&
                e.Message == "Incorrect password entered"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesFails()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
        
        var updatePasswordDto = new UpdatePasswordDto(OldPassword, NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock
            .Setup(x => x.Verify(OldPassword, HashedPassword))
            .Returns(true);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(NewPassword))
            .Returns(NewHashedPassword);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(NewHashedPassword, user.PasswordHash); 
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(OldPassword, HashedPassword), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(NewPassword), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId &&
                e.ActionType == ActionType.Update &&
                e.Message == "Password reset"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEvents_WithDifferentCorrelationIds()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
        
        var updatePasswordDto = new UpdatePasswordDto(OldPassword, NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        var correlationIds = new List<Guid>();
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock
            .Setup(x => x.Verify(OldPassword, HashedPassword))
            .Returns(true);
            
        _passwordHasherMock
            .Setup(x => x.HashPassword(NewPassword))
            .Returns(NewHashedPassword);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((SystemActionEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(correlationIds); 
        Assert.Equal(ActionType.Update, ((SystemActionEvent)_publisherMock.Invocations[0].Arguments[0]).ActionType);
    }

    [Fact]
    public async Task Handler_CallsSaveChangesTwice_When_IncorrectPassword()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
        
        var updatePasswordDto = new UpdatePasswordDto("wrong_password", NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock
            .Setup(x => x.Verify("wrong_password", HashedPassword))
            .Returns(false);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotPublishUpdateEvent_When_OldPasswordIncorrect()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
        
        var updatePasswordDto = new UpdatePasswordDto("wrong_password", NewPassword, ConfirmNewPassword);
        var command = new UpdatePasswordCommand(_testUserId, updatePasswordDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock
            .Setup(x => x.Verify("wrong_password", HashedPassword))
            .Returns(false);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => e.ActionType == ActionType.IncorrectPasswordAttempt),
            CancellationToken.None), Times.Once);
            
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => e.ActionType == ActionType.Update),
            CancellationToken.None), Times.Never);
    }
}