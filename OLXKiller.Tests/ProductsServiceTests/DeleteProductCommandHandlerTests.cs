using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using ProductsService.Application.Features.Products.Commands.Delete;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace OLXKiller.Tests.ProductsServiceTests;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductsRepository> _productsRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly DeleteProductCommandHandler _handler;
    
    private readonly Guid _testInitiatorUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Product _testProduct;
    
    public DeleteProductCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(ProductsService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteProductCommandHandler(
            _productsRepositoryMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);

        _testProduct = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            UserId = Guid.NewGuid(), 
            Price = 99.99m,
            StockQuantity = 10
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ProductDeletedSuccessfully()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.Delete(_testProduct));
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.Delete(_testProduct), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testInitiatorUserId &&
                e.ActionType == ActionType.Delete &&
                e.SenderServiceName == "ProductsService" &&
                e.Message.Contains($"Product {_testProductId} deleted")),
            CancellationToken.None), Times.Once);
            
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductDeletedEvent>(e => 
                e.ProductId == _testProductId &&
                e.SenderServiceName == "ProductsService"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ProductDoesNotExist()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("Product was not found", result.Description);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DeletesCorrectProduct()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        Product? deletedProduct = null;
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.Delete(It.IsAny<Product>()))
            .Callback<Product>(product => deletedProduct = product);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(deletedProduct);
        Assert.Equal(_testProductId, deletedProduct.Id);
        Assert.Equal(_testProduct.Name, deletedProduct.Name);
        Assert.Equal(_testProduct.UserId, deletedProduct.UserId);
    }

    [Fact]
    public async Task Handler_PublishesEventsWithSameCorrelationId()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        var correlationIds = new List<Guid>();
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((SystemActionEvent)e).CorrelationId));
                
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<ProductDeletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((ProductDeletedEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, correlationIds.Count);
        Assert.Equal(correlationIds[0], correlationIds[1]); 
    }

    [Fact]
    public async Task Handler_PublishesEvents_EvenIfSaveChangesThrows()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.Delete(_testProduct));
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Save failed", exception.Message);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ProductDeletedEvent>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DoesNotPublishEvents_When_ProductNotFound()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UsesInitiatorUserId_ForSystemActionEvent()
    {
        // Arrange
        var differentInitiatorUserId = Guid.NewGuid();
        var command = new DeleteProductCommand(differentInitiatorUserId, _testProductId);
        
        Guid? publishedUserId = null;
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                publishedUserId = ((SystemActionEvent)e).UserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(differentInitiatorUserId, publishedUserId); 
    }

    [Fact]
    public async Task Handler_DeletesProduct_WithDifferentOwner()
    {
        // Arrange
        var productOwnerId = Guid.NewGuid(); 
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            UserId = productOwnerId 
        };
        
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _productsRepositoryMock.Verify(x => x.Delete(product), Times.Once);
    }

    [Fact]
    public async Task Handler_HandlesMultipleCalls_ForSameProduct()
    {
        // Arrange
        var command = new DeleteProductCommand(_testInitiatorUserId, _testProductId);
        
        var callCount = 0;
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? _testProduct : null); 
            
        _productsRepositoryMock
            .Setup(x => x.Delete(_testProduct));
            
        _productsRepositoryMock
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
        
        _productsRepositoryMock.Verify(x => x.Delete(_testProduct), Times.Once); 
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once); 
    }
}