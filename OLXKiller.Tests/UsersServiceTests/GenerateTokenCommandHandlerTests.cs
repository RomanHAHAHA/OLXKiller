using System.Net;
using Moq;
using UsersService.Application.Features.Accounts.GenerateToken;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class GenerateTokenCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    
    private readonly GenerateTokenCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private const string ExpectedToken = "jwt_token_123";
    
    public GenerateTokenCommandHandlerTests()
    {
        _handler = new GenerateTokenCommandHandler(
            _usersRepositoryMock.Object,
            _jwtProviderMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_UserExists()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true
        };
        
        var command = new GenerateTokenCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _jwtProviderMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExpectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ExpectedToken, result.Data);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _jwtProviderMock.Verify(x => x.GenerateTokenAsync(user, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = new GenerateTokenCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _jwtProviderMock.Verify(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new GenerateTokenCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _jwtProviderMock.Verify(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_JwtProviderThrows()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true
        };
        
        var command = new GenerateTokenCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _jwtProviderMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("JWT generation failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("JWT generation failed", exception.Message);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _jwtProviderMock.Verify(x => x.GenerateTokenAsync(user, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true
        };
        
        var command = new GenerateTokenCommand(_testUserId);
        var cancellationToken = new CancellationToken(true);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, cancellationToken))
            .ReturnsAsync(user);
            
        _jwtProviderMock
            .Setup(x => x.GenerateTokenAsync(user, cancellationToken))
            .ReturnsAsync(ExpectedToken);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ExpectedToken, result.Data);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, cancellationToken), Times.Once);
        _jwtProviderMock.Verify(x => x.GenerateTokenAsync(user, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_CallsJwtProviderWithCorrectUser()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true
        };
        
        var command = new GenerateTokenCommand(_testUserId);
        
        User? capturedUser = null;
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _jwtProviderMock
            .Setup(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync(ExpectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedUser);
        Assert.Equal(_testUserId, capturedUser.Id);
        Assert.Equal(user.Email, capturedUser.Email);
        Assert.Equal(user.NickName, capturedUser.NickName);
    }

    [Fact]
    public async Task Handler_ReturnsEmptyToken_When_JwtProviderReturnsEmpty()
    {
        // Arrange
        var user = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true
        };
        
        var command = new GenerateTokenCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _jwtProviderMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Data);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _jwtProviderMock.Verify(x => x.GenerateTokenAsync(user, CancellationToken.None), Times.Once);
    }
}