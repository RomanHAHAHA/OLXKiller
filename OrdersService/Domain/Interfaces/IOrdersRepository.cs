using System.Data;
using Common.Domain.Dtos;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Application.Features.Orders.GetPagedOrders;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Interfaces;

public interface IOrdersRepository : IRepository<Order, Guid>
{
    Task<PagedList<OrderDto>> GetPagedOrdersAsync(
        OrderFilter orderFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken);
    
    Task<List<PersonalOrderDto>> GetPersonalOrdersAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}