using Microsoft.Extensions.Logging;
using Moq;
using ProductsService.Application.Features.Products.Commands.UpdateProductRating;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace OLXKiller.Tests.ProductsServiceTests;

public class UpdateProductRatingCommandHandlerTests
{
    private readonly Mock<IProductsRepository> _productsRepositoryMock = new();
    private readonly Mock<ILogger<UpdateProductRatingCommandHandler>> _loggerMock = new();
    
    private readonly UpdateProductRatingCommandHandler _handler;
    
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Product _testProduct;
    
    public UpdateProductRatingCommandHandlerTests()
    {
        _handler = new UpdateProductRatingCommandHandler(
            _productsRepositoryMock.Object,
            _loggerMock.Object);

        _testProduct = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            StockQuantity = 10,
            AverageRating = 3.5 
        };
    }

    [Fact]
    public async Task Handler_UpdatesRatingSuccessfully()
    {
        // Arrange
        const double newRating = 4.7;
        var command = new UpdateProductRatingCommand(_testProductId, newRating);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newRating, _testProduct.AverageRating);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Product {_testProductId} rating updated!")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handler_LogsInfo_When_ProductNotFound()
    {
        // Arrange
        var command = new UpdateProductRatingCommand(_testProductId, 4.5);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Product with id {_testProductId} not found")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handler_LogsFailure_When_SaveChangesFails()
    {
        // Arrange
        var command = new UpdateProductRatingCommand(_testProductId, 4.5);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(4.5, _testProduct.AverageRating); 
        
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to update product with {_testProductId}!")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handler_UpdatesRatingFromZero()
    {
        // Arrange
        var productWithZeroRating = new Product
        {
            Id = _testProductId,
            Name = "Test Product",
            AverageRating = 0.0 
        };
        
        var newRating = 4.2;
        var command = new UpdateProductRatingCommand(_testProductId, newRating);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productWithZeroRating);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newRating, productWithZeroRating.AverageRating);
    }

    [Fact]
    public async Task Handler_UpdatesRatingToZero()
    {
        // Arrange
        const double newRating = 0.0;
        var command = new UpdateProductRatingCommand(_testProductId, newRating);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0.0, _testProduct.AverageRating);
    }

    [Fact]
    public async Task Handler_HandlesMaxRating()
    {
        // Arrange
        var maxRating = 5.0;
        var command = new UpdateProductRatingCommand(_testProductId, maxRating);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5.0, _testProduct.AverageRating);
    }

    [Fact]
    public async Task Handler_HandlesNegativeRating()
    {
        // Arrange
        const double negativeRating = -1.5;
        var command = new UpdateProductRatingCommand(_testProductId, negativeRating);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(negativeRating, _testProduct.AverageRating); 
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new UpdateProductRatingCommand(_testProductId, 4.5);
        var cancellationToken = new CancellationToken(true);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, cancellationToken))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, cancellationToken), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new UpdateProductRatingCommand(_testProductId, 4.5);
        
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
    }

    [Fact]
    public async Task Handler_DoesNotChangeOtherProductProperties()
    {
        // Arrange
        var originalName = _testProduct.Name;
        var originalPrice = _testProduct.Price;
        var originalStock = _testProduct.StockQuantity;
        
        var command = new UpdateProductRatingCommand(_testProductId, 4.5);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(4.5, _testProduct.AverageRating);
        Assert.Equal(originalName, _testProduct.Name); 
        Assert.Equal(originalPrice, _testProduct.Price); 
        Assert.Equal(originalStock, _testProduct.StockQuantity); 
    }

    [Fact]
    public async Task Handler_UpdatesMultipleTimes()
    {
        // Arrange
        var command1 = new UpdateProductRatingCommand(_testProductId, 4.0);
        var command2 = new UpdateProductRatingCommand(_testProductId, 4.5);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act 
        await _handler.Handle(command1, CancellationToken.None);
        
        // Assert 
        Assert.Equal(4.0, _testProduct.AverageRating);
        
        // Act 
        await _handler.Handle(command2, CancellationToken.None);
        
        // Assert 
        Assert.Equal(4.5, _testProduct.AverageRating);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}