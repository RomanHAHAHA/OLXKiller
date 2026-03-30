using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProductsService.Application.Features.Products.Commands.CheckStockQuantity;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace OLXKiller.Tests.ProductsServiceTests;

public class CheckProductStockQuantityCommandHandlerTests
{
    private readonly Mock<IProductsRepository> _productsRepositoryMock = new();
    private readonly Mock<ILogger<CheckProductStockQuantityCommandHandler>> _loggerMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly CheckProductStockQuantityCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    
    public CheckProductStockQuantityCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(ProductsService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CheckProductStockQuantityCommandHandler(
            _productsRepositoryMock.Object,
            _loggerMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);
    }

    [Fact]
    public async Task Handler_LogsAndReturns_When_ProductNotFound()
    {
        // Arrange
        const int requestedQuantity = 5;
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Product with id {_testProductId} was not found")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEvent_When_StockQuantityExceeded()
    {
        // Arrange
        const int productStockQuantity = 3;
        const int requestedQuantity = 5;
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            StockQuantity = productStockQuantity
        };
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductStockExceededEvent>(e => 
                e.UserId == _testUserId &&
                e.StockQuantity == productStockQuantity &&
                e.SenderServiceName == "ProductsService"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotPublishEvent_When_StockQuantitySufficient()
    {
        // Arrange
        const int productStockQuantity = 10;
        const int requestedQuantity = 5; 
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            StockQuantity = productStockQuantity
        };
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DoesNotPublishEvent_When_StockQuantityEqual()
    {
        // Arrange
        const int productStockQuantity = 5;
        const int requestedQuantity = 5; 
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            StockQuantity = productStockQuantity
        };
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        const int requestedQuantity = 5;
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handler_PublishesEventWithNewCorrelationId()
    {
        // Arrange
        const int productStockQuantity = 3;
        const int requestedQuantity = 5;
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            StockQuantity = productStockQuantity
        };
        
        var correlationIds = new List<Guid>();
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<ProductStockExceededEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((ProductStockExceededEvent)e).CorrelationId));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(correlationIds);
        Assert.NotEqual(Guid.Empty, correlationIds[0]);
    }

    [Fact]
    public async Task Handler_HandlesLargeRequestedQuantity()
    {
        // Arrange
        const int productStockQuantity = int.MaxValue - 1;
        const int requestedQuantity = int.MaxValue; 
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            StockQuantity = productStockQuantity
        };
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // int.MaxValue - 1 < int.MaxValue = true, должно опубликовать событие
        _publisherMock.Verify(x => x.Publish(It.IsAny<ProductStockExceededEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_HandlesNegativeRequestedQuantity()
    {
        // Arrange
        const int productStockQuantity = 5;
        const int requestedQuantity = -1; 
        var command = new CheckProductStockQuantityCommand(_testUserId, _testProductId, requestedQuantity);
        
        var product = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            StockQuantity = productStockQuantity
        };
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}