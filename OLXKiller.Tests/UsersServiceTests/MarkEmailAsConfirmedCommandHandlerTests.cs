using System.Net;
using Moq;
using UsersService.Application.Features.Accounts.MarkEmailConfirmed;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class MarkEmailAsConfirmedCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly MarkEmailAsConfirmedCommandHandler _handler;
    
    private const string TestEmail = "test@example.com";
    private readonly User _testUser;
    
    public MarkEmailAsConfirmedCommandHandlerTests()
    {
        _handler = new MarkEmailAsConfirmedCommandHandler(_usersRepositoryMock.Object);
        
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = TestEmail,
            NickName = "TestUser",
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_EmailMarkedAsConfirmed()
    {
        // Arrange
        var command = new MarkEmailAsConfirmedCommand(TestEmail);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.True(_testUser.EmailConfirmed); 
        
        _usersRepositoryMock.Verify(x => x.GetByEmailAsync(TestEmail, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = new MarkEmailAsConfirmedCommand("nonexistent@example.com");
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("nonexistent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Description);
        
        _usersRepositoryMock.Verify(x => x.GetByEmailAsync("nonexistent@example.com", CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_EmailAlreadyConfirmed()
    {
        // Arrange
        var alreadyConfirmedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = TestEmail,
            NickName = "TestUser",
            EmailConfirmed = true 
        };
        
        var command = new MarkEmailAsConfirmedCommand(TestEmail);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alreadyConfirmedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.True(alreadyConfirmedUser.EmailConfirmed);
        
        _usersRepositoryMock.Verify(x => x.GetByEmailAsync(TestEmail, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); 
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesFails()
    {
        // Arrange
        var command = new MarkEmailAsConfirmedCommand(TestEmail);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal("Failed to update user", result.Description);
        Assert.True(_testUser.EmailConfirmed); 
        
        _usersRepositoryMock.Verify(x => x.GetByEmailAsync(TestEmail, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_UpdatesOnlyEmailConfirmedProperty()
    {
        // Arrange
        var originalUser = new User
        {
            Id = Guid.NewGuid(),
            Email = TestEmail,
            NickName = "OriginalName",
            PasswordHash = "original_hash",
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        
        var command = new MarkEmailAsConfirmedCommand(TestEmail);

        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(originalUser.EmailConfirmed); 
        Assert.Equal("OriginalName", originalUser.NickName); 
        Assert.Equal("original_hash", originalUser.PasswordHash); 
        Assert.Equal(TestEmail, originalUser.Email); 
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new MarkEmailAsConfirmedCommand(TestEmail);
        var cancellationToken = new CancellationToken(true);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, cancellationToken))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _usersRepositoryMock.Verify(x => x.GetByEmailAsync(TestEmail, cancellationToken), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new MarkEmailAsConfirmedCommand(TestEmail);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _usersRepositoryMock.Verify(x => x.GetByEmailAsync(TestEmail, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_HandlesCaseSensitiveEmail()
    {
        // Arrange
        const string upperCaseEmail = "TEST@example.com";

        var command = new MarkEmailAsConfirmedCommand(upperCaseEmail); 
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(upperCaseEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null); 

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
    }

    [Fact]
    public async Task Handler_DoesNotSave_When_NoChangesNeeded()
    {
        // Arrange
        var alreadyConfirmedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = TestEmail,
            EmailConfirmed = true
        };
        
        var command = new MarkEmailAsConfirmedCommand(TestEmail);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alreadyConfirmedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsCorrectErrorMessages()
    {
        // Arrange
        var command = new MarkEmailAsConfirmedCommand("wrong@example.com");
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync("wrong@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Description);
        
        var command2 = new MarkEmailAsConfirmedCommand(TestEmail);
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(TestEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result2 = await _handler.Handle(command2, CancellationToken.None);
        
        Assert.False(result2.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result2.Status);
        Assert.Equal("Failed to update user", result2.Description);
    }
}