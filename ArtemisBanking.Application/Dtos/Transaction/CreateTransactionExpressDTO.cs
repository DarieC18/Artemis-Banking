namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class CreateTransactionExpressDTO
    {
        public string CuentaOrigen { get; set; }
        public string CuentaDestino { get; set; }
        public decimal Monto { get; set; }
        public string UserId { get; set; }
    }
}