namespace BTGPactual.Shared.Extensions;

public static class DecimalExtensions
{
    public static string ToCurrencyString(this decimal amount)
    {
        return $"COP ${amount:N0}";
    }
}