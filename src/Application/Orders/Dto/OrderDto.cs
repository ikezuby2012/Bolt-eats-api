using System.Text.Json.Serialization;
using Application.Restaurant.Dto;
using Application.Users.Dto;
using SharedKernel;

namespace Application.Orders.Dto;

public class OrderDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public CreatedUserDto? Customer { get; set; }
    public Guid RestaurantId { get; set; }
    public RestaurantDto? Restaurant { get; set; }
    public Guid? RiderId { get; set; }
    public CreatedUserDto? Rider { get; set; }
    public Guid AddressId { get; set; }
    public AddressDto? Address { get; set; }
    public string OrderStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal? Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? PromoCode { get; set; }
    public string? PaymentRef { get; set; }
    public string? Notes { get; set; }
    public DateTime? CheckoutAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator OrderDto(Domain.Order.Order order) => new OrderDto
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        RestaurantId = order.RestaurantId,
        RiderId = order.RiderId,
        AddressId = order.AddressId,
        OrderStatus= order.OrderStatusId is int statusId ? Domain.Order.EOrderStatus.FromValue(statusId)!.Name : "",
        SubTotal = order.SubTotal,
        DeliveryFee = order.DeliveryFee,
        Discount = order.Discount,
        Tax = order.Tax,
        Total = order.Total,
        PromoCode = order.PromoCode,
        PaymentRef = order.PaymentRef,
        Notes = order.Notes,
        CheckoutAt = order.CheckoutAt,
        AcceptedAt = order.AcceptedAt,
        PickedUpAt = order.PickedUpAt,
        DeliveredAt = order.DeliveredAt,
        CreatedAt = order.CreatedAt,
        CreatedBy = order.CreatedBy,
        UpdatedAt = order.UpdatedAt,
        UpdatedBy = order.UpdatedBy,

        Customer = order.Customer != null ? (CreatedUserDto)order.Customer : null,
        Rider = order.Rider != null ? (CreatedUserDto)order.Rider : null,
        Restaurant = order.Restaurant != null ? (RestaurantDto)order.Restaurant : null,
        Address = order.Address != null ? (AddressDto)order.Address : null
    };
}
