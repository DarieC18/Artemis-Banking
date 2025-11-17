namespace ArtemisBanking.Domain.Enums
{
    public enum TransactionOperationType
    {
        Desconocida = 0,
        Deposito = 1,
        Retiro = 2,
        PagoTarjetaCredito = 3,
        PagoPrestamo = 4,
        TransferenciaTerceros = 5
    }
}
