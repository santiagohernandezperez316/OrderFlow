using AutoMapper;
using OrderFlow.Application.Orders.Query.Dto;

namespace OrderFlow.Application.Orders;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Domain.Orders.Entity.Order, OrderDto>()
            .ForCtorParam(nameof(OrderDto.CreadoEn), opt => opt.MapFrom(src => src.CreatedOn));
    }
}
