using CartsService.Domain.Entities;
using MediatR;

namespace CartsService.Application.Features.Products.RollBackUpdate;

public record ProductUpdateRollbackCommand(ProductSnapshot Snapshot) : IRequest;