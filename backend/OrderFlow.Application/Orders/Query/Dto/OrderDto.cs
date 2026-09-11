using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Application.Orders.Query.Dto;

public record OrderDto(Guid Id, string ClienteNombre, string Sku, int Cantidad, OrderStatus Status, DateTime CreadoEn, string? RejectionReason);
