namespace ArtemisBanking.Application.Dtos.SavingsAccount
{
    // DTO para respuesta de GET /api/savings-account 
    public class SavingsAccountApiListItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string NumeroCuenta { get; set; } = string.Empty; // 9 digitos
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Tipo { get; set; } = string.Empty; // principal | secundaria
    }

    public class SavingsAccountApiListResponseDTO
    {
        public List<SavingsAccountApiListItemDTO> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // DTO para POST /api/savings-account
    public class SavingsAccountApiCreateRequestDTO
    {
        public string ClienteId { get; set; } = string.Empty;
        public decimal BalanceInicial { get; set; } // puede ser 0
    }

    // DTO para GET /api/savings-account/{accountNumber}/transactions 
    public class SavingsAccountApiTransactionsResponseDTO
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public List<SavingsAccountApiTransactionDTO> Transacciones { get; set; } = new();
    }

    public class SavingsAccountApiTransactionDTO
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty; // DEBITO | CREDITO
        public string Beneficiario { get; set; } = string.Empty; // cuenta destino / ultimos 4 dígitos TC / numero préstamo / 'RETIRO'
        public string Origen { get; set; } = string.Empty; // cuenta origen / ultimos 4 díigitos TC / numero prestamo / 'DEPSITO'
        public string Estado { get; set; } = string.Empty; // APROBADA | RECHAZADA
    }
}

