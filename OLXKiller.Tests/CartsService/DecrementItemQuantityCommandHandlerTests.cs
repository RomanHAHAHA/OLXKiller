using CartsService.Application.Features.CartItems.Decrement;
using CartsService.Domain.Entities;
using CartsService.Domain.Interfaces;
using Moq;

namespace OLXKiller.Tests.CartsService;

public class DecrementItemQuantityCommandHandlerTests
{
    private readonly Mock<ICartsRepository> _repositoryMock;
    private readonly DecrementItemQuantityCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    
    public DecrementItemQuantityCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICartsRepository>();
        _handler = new DecrementItemQuantityCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_QuantityDecremented()
    {
        // Arrange
        var cartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 5
        };
        
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);
        
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
            
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, cartItem.Quantity);
        
        _repositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_CartItemDoesNotExist()
    {
        // Arrange
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);
        
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("CartItem was not found", result.Description);
        
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_QuantityIsOne()
    {
        // Arrange
        var cartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 1
        };
        
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);
        
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Can`t decrement item quantity because it is already 1.", result.Description);
        Assert.Equal(1, cartItem.Quantity);
        
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DecrementsFromTwoToOne()
    {
        // Arrange
        var cartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 2
        };
        
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);
        
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
            
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, cartItem.Quantity);
    }

    [Fact]
    public async Task Handler_DecrementsFromLargeQuantity()
    {
        // Arrange
        var cartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 100
        };
        
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);
        
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
            
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99, cartItem.Quantity);
    }

    [Fact]
    public async Task Handler_ReturnsCorrectCartItem()
    {
        // Arrange
        var cartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 3
        };
        
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
            
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, cartItem.Quantity);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var cartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 3
        };
        
        var command = new DecrementItemQuantityCommand(_testUserId, _testProductId);
        var cancellationToken = new CancellationToken(true);
        
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, cancellationToken))
            .ReturnsAsync(cartItem);
            
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _repositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, cancellationToken), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
}