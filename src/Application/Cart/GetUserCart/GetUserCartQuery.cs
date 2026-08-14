using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using SharedKernel;

namespace Application.Cart.GetUserCart;

public sealed record GetUserCartQuery : IQuery<CartDto>;
