using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using ProductsService.Application.Common.Dtos;
using ProductsService.Application.Features.Products.Commands.Create;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace OLXKiller.Tests.ProductsServiceTests;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductsRepository> _productsRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly CreateProductCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _expectedProductId = Guid.NewGuid();
    private const string ProductName = "Test Product";
    private const string ProductDescription = "Test Description";
    private const decimal ProductPrice = 99.99m;
    private const int StockQuantity = 10;
    
    public CreateProductCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(ProductsService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateProductCommandHandler(
            _productsRepositoryMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ProductCreatedSuccessfully()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        Product? capturedProduct = null;
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => 
            {
                capturedProduct = product;
                product.GetType().GetProperty("Id")!.SetValue(product, _expectedProductId);
            })
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_expectedProductId, result.Data);
        Assert.NotNull(capturedProduct);
        Assert.Equal(ProductName, capturedProduct.Name);
        Assert.Equal(ProductPrice, capturedProduct.Price);
        Assert.Equal(ProductDescription, capturedProduct.Description);
        Assert.Equal(StockQuantity, capturedProduct.StockQuantity);
        Assert.Equal(_testUserId, capturedProduct.UserId);
        
        _productsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Product>(), CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId &&
                e.ActionType == ActionType.Create &&
                e.SenderServiceName == "ProductsService" &&
                e.Message.Contains($"Product {_expectedProductId} created")),
            CancellationToken.None), Times.Once);
            
        _publisherMock.Verify(x => x.Publish(
            It.Is<ProductCreatedEvent>(e => 
                e.ProductId == _expectedProductId &&
                e.SellerId == _testUserId &&
                e.SenderServiceName == "ProductsService" &&
                e.Name == ProductName &&
                e.Price == ProductPrice),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesFails()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _productsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Product>(), CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ProductCreatedEvent>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEventsWithSameCorrelationId()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        var correlationIds = new List<Guid>();
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => 
                product.GetType().GetProperty("Id")!.SetValue(product, _expectedProductId))
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((SystemActionEvent)e).CorrelationId));
                
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((ProductCreatedEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, correlationIds.Count);
        Assert.Equal(correlationIds[0], correlationIds[1]); 
    }
    
    [Fact]
    public async Task Handler_CreatesProductWithZeroPriceAndStock()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, 0m, 0);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        Product? capturedProduct = null;
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => 
            {
                capturedProduct = product;
                product.GetType().GetProperty("Id")!.SetValue(product, _expectedProductId);
            })
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProduct);
        Assert.Equal(0m, capturedProduct.Price); 
        Assert.Equal(0, capturedProduct.StockQuantity); 
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _productsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Product>(), CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_SetsProductCreationTimestamp()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        Product? capturedProduct = null;
        var testStartTime = DateTime.UtcNow;
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => 
            {
                capturedProduct = product;
                product.GetType().GetProperty("Id")!.SetValue(product, _expectedProductId);
            })
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProduct);
        Assert.InRange(capturedProduct.CreatedAt, testStartTime.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task Handler_ReturnsProductId_When_CreationSucceeds()
    {
        // Arrange
        var productCreateDto = new ProductCreateDto(ProductName, ProductDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        var customProductId = Guid.NewGuid();
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => 
                product.GetType().GetProperty("Id")!.SetValue(product, customProductId))
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(customProductId, result.Data);
    }

    [Fact]
    public async Task Handler_CreatesProductWithLongNameAndDescription()
    {
        // Arrange
        var longName = new string('A', 100);
        var longDescription = new string('B', 1000);
        var productCreateDto = new ProductCreateDto(longName, longDescription, ProductPrice, StockQuantity);
        var command = new CreateProductCommand(_testUserId, productCreateDto);
        
        Product? capturedProduct = null;
        
        _productsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => 
            {
                capturedProduct = product;
                product.GetType().GetProperty("Id")!.SetValue(product, _expectedProductId);
            })
            .Returns(Task.CompletedTask);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProduct);
        Assert.Equal(longName, capturedProduct.Name);
        Assert.Equal(longDescription, capturedProduct.Description);
    }
}