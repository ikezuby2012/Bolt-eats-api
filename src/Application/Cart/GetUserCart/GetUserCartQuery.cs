using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using SharedKernel;

namespace Application.Cart.GetUserCart;

public sealed record GetUserCartQuery(int PageSize = 1000,
    int pageNumber = 1) : IQuery<PaginatedResult<CartDto>>;
