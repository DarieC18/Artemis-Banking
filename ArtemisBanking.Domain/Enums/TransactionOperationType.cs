namespace ArtemisBanking.Domain.Enums
{
    public enum TransactionOperationType
    {
        Desconocida = 0,
        Deposito = 1,
        Retiro = 2,
        PagoTarjetaCredito = 3,
        PagoPrestamo = 4,
        TransferenciaTerceros = 5,

        CashierThirdPartyTransfer = 6,
        ClientExpressTransfer = 7,
        ClientBeneficiaryTransfer = 8,
        OwnAccountsTransfer = 9
    }
}
