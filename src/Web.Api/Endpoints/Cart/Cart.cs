using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Messaging;
using Application.Cart.AddCartItem;
using Application.Cart.ApplyPromoCode;
using Application.Cart.ClearCart;
using Application.Cart.DeleteCartItem;
using Application.Cart.Dto;
using Application.Cart.GetCartSummary;
using Application.Cart.GetUserCart;
using Application.Cart.RemovePromoCode;
using Application.Cart.UpdateCartItem;
using Application.Restaurant.UpdateCartItemQuantity;
using Domain.Cart;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Cart;

public class Cart : IEndpoint
{
    internal sealed record class AddCartItemRequest(Guid MenuItemId, int Quantity, string? Notes, string? PromoCode);
    internal sealed record UpdateCartItemRequest([Range(1, 1000)] int Quantity);
    internal sealed record ApplyPromoCodeRequest(string Code);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("cart", async (IQueryHandler<GetUserCartQuery, CartDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetUserCartQuery();

            Result<CartDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartDto>.Success(value, "All User Carts")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("GetCart").Produces<PaginatedResult<CartDto>>();

        app.MapGet("cart/summary", async (IQueryHandler<GetCartSummaryQuery, CartSummaryDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetCartSummaryQuery();

            Result<CartSummaryDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartSummaryDto>.Success(value, "All User Cart Summary")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("GetCartSummary").Produces<CartSummaryDto>();

        //**
        //Cart Items
        //
        //*/
        app.MapPost("cart/items", async ([FromBody] AddCartItemRequest param, ICommandHandler<AddCartItemCommand, CartDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new AddCartItemCommand(param.MenuItemId, param.Quantity, param.Notes, param.PromoCode);

            Result<CartDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartDto>.Success(value, "Cart Item created successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("AddCartItem").Produces<CartDto>();

        app.MapPut("cart/items/{itemId:guid}", async (Guid itemId, [FromBody] UpdateCartItemRequest param, ICommandHandler<UpdateCartItemCommand, CartDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new UpdateCartItemCommand(itemId, param.Quantity);

            Result<CartDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartDto>.Success(value, "Cart Item updated successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("UpdateCartItem").Produces<CartDto>().Produces(404);

        app.MapDelete("cart/items/{itemId:guid}", async (Guid itemId, ICommandHandler<DeleteCartItemCommand, CartDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new DeleteCartItemCommand(itemId);

            Result<CartDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartDto>.Success(value, "Cart Item deleted successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("DeleteCartItem").Produces<CartDto>().Produces(404);

        app.MapDelete("cart/{CartId:guid}", async (Guid CartId, ICommandHandler<ClearCartCommand> handler, CancellationToken cancellationToken) =>
        {
            var query = new ClearCartCommand(CartId);

            Result result = await handler.Handle(query, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("ClearCart").Produces<CartDto>().Produces(204);


        //**
        //Apply Promo
        //
        //*/
        app.MapPost("cart/{cartId:guid}/promo", async (Guid cartId, [FromBody] ApplyPromoCodeRequest param, ICommandHandler<ApplyPromoCodeCommand, CartSummaryDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new ApplyPromoCodeCommand(cartId, param.Code);

            Result<CartSummaryDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartSummaryDto>.Success(value, "Promo code applied to cart successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("ApplyPromoCode").Produces<CartDto>().Produces(404);

        app.MapDelete("cart/{cartId:guid}/promo", async (Guid cartId, ICommandHandler<RemovePromoCodeCommand, CartSummaryDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new RemovePromoCodeCommand(cartId);

            Result<CartSummaryDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<CartSummaryDto>.Success(value, "Promo code removed from cart successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Cart).RequireAuthorization().WithName("RemovePromoCode").Produces<CartDto>().Produces(404);

        // Endpoint

        // PATCH cart/items/{cartItemId}/increase
        app.MapPatch("cart/items/{cartItemId:guid}/increase", async (
            Guid cartItemId,
            ICommandHandler<UpdateCartItemQuantityCommand, CartDto> handler,
            CancellationToken ct) =>
        {
            Result<CartDto> result = await handler.Handle(
                new UpdateCartItemQuantityCommand(cartItemId, QuantityAction.Increase), ct);

            return result.Match(
                value => Results.Ok(ApiResponse<CartDto>.Success(value)),
                error => CustomResults.Problem(error));
        })
        .WithName("IncreaseCartItemQuantity")
        .WithTags(Tags.Cart)
        .RequireAuthorization()
        .Produces<ApiResponse<CartDto>>()
        .Produces(404);

        // PATCH cart/items/{cartItemId}/decrease
        app.MapPatch("cart/items/{cartItemId:guid}/decrease", async (
            Guid cartItemId,
            ICommandHandler<UpdateCartItemQuantityCommand, CartDto> handler,
            CancellationToken ct) =>
        {
            Result<CartDto> result = await handler.Handle(
                new UpdateCartItemQuantityCommand(cartItemId, QuantityAction.Decrease), ct);

            return result.Match(
                value => Results.Ok(ApiResponse<CartDto>.Success(value)),
                error => CustomResults.Problem(error));
        })
        .WithName("DecreaseCartItemQuantity")
        .WithTags(Tags.Cart)
        .RequireAuthorization()
        .Produces<ApiResponse<CartDto>>()
        .Produces(404);
    }
}

