using Microsoft.EntityFrameworkCore;
using OLXKiller.Application.Abstractions;
using OLXKiller.Domain.Abstractions.Models;
using OLXKiller.Domain.Abstractions.Repositories;
using OLXKiller.Domain.Entities;
using OLXKiller.Domain.Models;
using OLXKiller.Persistence.Abstractions;
using System.Net;

namespace OLXKiller.Application.Services;

public class CartsService(
    IProductsRepository _productsRepository,
    IUsersRepository _usersRepository,
    ICartItemsRepository _cartItemsRepository,
    IUnitOfWork _unitOfWork) : ICartsService
{
    public async Task<IBaseResponse> AddToCart(Guid productId, Guid userId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await _productsRepository.GetByIdAsync(productId);
            
            if (product is null)
                return new BaseResponse(HttpStatusCode.NotFound, "Product was not found");

            if (product.Amount == 0)
                return new BaseResponse(HttpStatusCode.Conflict, "Product is out of stock");

            var cartItem = await _cartItemsRepository.GetByIdAsync(userId, productId);

            if (cartItem is null)
            {
                var user = await _usersRepository.GetByIdAsync(userId);
            
                if (user is null)
                    return new BaseResponse(HttpStatusCode.NotFound, "User was not found");

                var newCartItem = new CartItemEntity
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                };

                await _cartItemsRepository.CreateAsync(newCartItem);
            }
            else
            {
                cartItem.Quantity += 1;
                await _cartItemsRepository.UpdateAsync(cartItem);
            }

            product.Amount -= 1;
            await _productsRepository.UpdateAsync(product);
            await transaction.CommitAsync();

            return new BaseResponse(HttpStatusCode.OK);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return new BaseResponse(HttpStatusCode.InternalServerError, exception.Message);
        }
    }

    public async Task<IBaseResponse> RemoveFromCart(Guid productId, Guid userId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var cartItem = await _cartItemsRepository.GetByIdAsync(userId, productId);
           
            if (cartItem is null)
                return new BaseResponse(HttpStatusCode.NotFound, "Cart item was not found");

            var product = await _productsRepository.GetByIdAsync(productId);
            
            if (product is null)
                return new BaseResponse(HttpStatusCode.NotFound, "Product was not found");

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity -= 1;
                await _cartItemsRepository.UpdateAsync(cartItem);
            }
            else
            {
                await _cartItemsRepository.RemoveAsync(cartItem);
            }

            product.Amount += 1;
            await _productsRepository.UpdateAsync(product);
            await transaction.CommitAsync();

            return new BaseResponse(HttpStatusCode.OK);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return new BaseResponse(HttpStatusCode.InternalServerError, exception.Message);
        }
    }


    public async Task<IEnumerable<CartItemEntity>> GetItems(Guid userId)
    {
        return await _cartItemsRepository.GetItemsByUserIdAsync(userId);
    }
}
