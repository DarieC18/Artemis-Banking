using ArtemisBanking.Domain.Enums;

namespace ArtemisBanking.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int SavingsAccountId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Tipo { get; set; }
        public string Beneficiario { get; set; }
        public string Origen { get; set; }
        public string Estado { get; set; }
        public virtual SavingsAccount SavingsAccount { get; set; }
        public TransactionOperationType OperationType { get; set; } = TransactionOperationType.Desconocida;
        public string OperatedByUserId { get; set; } = default!;

    }
}