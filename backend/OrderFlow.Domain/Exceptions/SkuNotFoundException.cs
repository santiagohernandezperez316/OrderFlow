namespace OrderFlow.Domain.Exceptions;

public sealed class SkuNotFoundException : CoreBusinessException
{
    public SkuNotFoundException()
    {
    }

    public SkuNotFoundException(string message) : base(message)
    {
    }

    public SkuNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}
