namespace OrderFlow.Domain.Exceptions;

public class CoreBusinessException : Exception
{
    public CoreBusinessException()
    {

    }
    public CoreBusinessException(string message) : base(message)
    {
    }

    public CoreBusinessException(string message, Exception inner) : base(message, inner)
    {
    }

}
