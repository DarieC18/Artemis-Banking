namespace ArtemisBanking.Application.Dtos.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Tipo { get; set; }
        public string Beneficiario { get; set; }
        public string Origen { get; set; }
        public string Estado { get; set; }
    }
}