namespace BTGPactual.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string fundName)
        : base($"No tiene saldo disponible para vincularse al fondo {fundName}")
    {
    }
}