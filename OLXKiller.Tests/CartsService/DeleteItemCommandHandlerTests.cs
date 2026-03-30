using System.Net;
using CartsService.Application.Features.CartItems.Delete;
using CartsService.Domain.Entities;
using CartsService.Domain.Interfaces;
using Moq;

namespace OLXKiller.Tests.CartsService;

public class DeleteItemCommandHandlerTests
{
    private readonly Mock<ICartsRepository> _cartsRepositoryMock = new();
    private readonly DeleteItemCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly CartItem _testCartItem;
    
    public DeleteItemCommandHandlerTests()
    {
        _handler = new DeleteItemCommandHandler(_cartsRepositoryMock.Object);

        _testCartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 3
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ItemDeletedSuccessfully()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.Delete(_testCartItem));
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, CancellationToken.None), Times.Once);
        _cartsRepositoryMock.Verify(x => x.Delete(_testCartItem), Times.Once);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_CartItemDoesNotExist()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("CartItem was not found", result.Description);
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, CancellationToken.None), Times.Once);
        _cartsRepositoryMock.Verify(x => x.Delete(It.IsAny<CartItem>()), Times.Never);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DeletesCorrectCartItem()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        CartItem? deletedItem = null;
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.Delete(It.IsAny<CartItem>()))
            .Callback<CartItem>(item => deletedItem = item);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(deletedItem);
        Assert.Equal(_testUserId, deletedItem.UserId);
        Assert.Equal(_testProductId, deletedItem.ProductId);
        Assert.Equal(3, deletedItem.Quantity);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, CancellationToken.None), Times.Once);
        _cartsRepositoryMock.Verify(x => x.Delete(It.IsAny<CartItem>()), Times.Never);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        var cancellationToken = new CancellationToken(true);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, cancellationToken))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.Delete(_testCartItem));
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, cancellationToken), Times.Once);
        _cartsRepositoryMock.Verify(x => x.Delete(_testCartItem), Times.Once);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotCallDelete_When_ItemNotFound()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _cartsRepositoryMock.Verify(x => x.Delete(It.IsAny<CartItem>()), Times.Never);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_DeleteAndSaveSucceed()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        var deleteCalled = false;
        var saveChangesCalled = false;
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.Delete(It.IsAny<CartItem>()))
            .Callback(() => deleteCalled = true);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveChangesCalled = true)
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(deleteCalled);
        Assert.True(saveChangesCalled);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_SaveChangesThrows()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.Delete(_testCartItem));
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Save failed", exception.Message);
        
        _cartsRepositoryMock.Verify(x => x.Delete(_testCartItem), Times.Once);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_HandlesMultipleCalls_ForSameItem()
    {
        // Arrange
        var command = new DeleteItemCommand(_testUserId, _testProductId);
        
        var callCount = 0;
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? _testCartItem : null); 
            
        _cartsRepositoryMock
            .Setup(x => x.Delete(_testCartItem));
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act 
        var result1 = await _handler.Handle(command, CancellationToken.None);

        // Assert 
        Assert.True(result1.IsSuccess);
        
        // Act 
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert 
        Assert.False(result2.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result2.Status);
        
        _cartsRepositoryMock.Verify(x => x.Delete(_testCartItem), Times.Once); 
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once); 
    }
}