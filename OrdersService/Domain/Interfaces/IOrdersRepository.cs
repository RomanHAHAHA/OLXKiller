using System.Data;
using Common.Domain.Dtos;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Application.Features.Orders.Queries.GetOrderDetails;
using OrdersService.Application.Features.Orders.Queries.GetPagedOrders;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Interfaces;

public interface IOrdersRepository : IRepository<Order, Guid>
{
    Task<PagedList<ShortOrdersDto>> GetPagedOrdersAsync(
        OrderFilter orderFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken);
    
    Task<List<PersonalOrderDto>> GetPersonalOrdersAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OrderDetailsDto?> GetOrderDetailsAsync(
        Guid orderId,
        CancellationToken cancellationToken);
    
    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}