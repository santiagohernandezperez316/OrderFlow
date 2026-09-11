using FluentValidation;
using OrderFlow.Application.Orders.Command;

namespace OrderFlow.Api.ApiHandlers;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.ClienteNombre)
            .NotEmpty();

        RuleFor(command => command.Sku)
            .NotEmpty();

        RuleFor(command => command.Cantidad)
            .InclusiveBetween(1, 100);
    }
}
