using System.Net;
using CartsService.Application.Features.CartItems.Increment;
using CartsService.Domain.Entities;
using CartsService.Domain.Interfaces;
using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.CartItem;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;

namespace OLXKiller.Tests.CartsService;

public class IncrementItemQuantityCommandHandlerTests
{
    private readonly Mock<ICartsRepository> _cartsRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly IncrementItemQuantityCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly CartItem _testCartItem;
    
    public IncrementItemQuantityCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(CartsService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _handler = new IncrementItemQuantityCommandHandler(
            _cartsRepositoryMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);

        _testCartItem = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 1
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_QuantityIncrementedSuccessfully()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.Equal(2, _testCartItem.Quantity); 
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, CancellationToken.None), Times.Once);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductCartQuantityIncrementedEvent>(e => 
                e.UserId == _testUserId &&
                e.ProductId == _testProductId &&
                e.RequestedQuantity == 2 &&
                e.SenderServiceName == "CartsService"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_CartItemDoesNotExist()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
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
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_IncrementsQuantity_FromAnyValue()
    {
        // Arrange
        var cartItemWithQuantity5 = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 5
        };
        
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItemWithQuantity5);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(6, cartItemWithQuantity5.Quantity);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductCartQuantityIncrementedEvent>(e => e.RequestedQuantity == 6),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEvent_WithCorrectQuantity()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
        int? publishedQuantity = null;
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<ProductCartQuantityIncrementedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                publishedQuantity = ((ProductCartQuantityIncrementedEvent)e).RequestedQuantity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, publishedQuantity); 
        Assert.Equal(2, _testCartItem.Quantity); 
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        var cancellationToken = new CancellationToken(true);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, cancellationToken))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, cancellationToken), Times.Once);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ProductCartQuantityIncrementedEvent>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _cartsRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, _testProductId, CancellationToken.None), Times.Once);
        _cartsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PublishesEvent_EvenIfSaveChangesThrows()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Save failed", exception.Message);
        Assert.Equal(2, _testCartItem.Quantity); 
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductCartQuantityIncrementedEvent>(e => e.RequestedQuantity == 2),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_QuantityIsMaxValue()
    {
        // Arrange
        var cartItemWithMaxQuantity = new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = int.MaxValue
        };
        
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItemWithMaxQuantity);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(int.MinValue, cartItemWithMaxQuantity.Quantity); 
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductCartQuantityIncrementedEvent>(e => e.RequestedQuantity == int.MinValue),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEvent_WithNewCorrelationId()
    {
        // Arrange
        var command = new IncrementItemQuantityCommand(_testUserId, _testProductId);
        var correlationIds = new List<Guid>();
        
        _cartsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, _testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testCartItem);
            
        _cartsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<ProductCartQuantityIncrementedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((ProductCartQuantityIncrementedEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(correlationIds);
        Assert.NotEqual(Guid.Empty, correlationIds[0]);
    }
}