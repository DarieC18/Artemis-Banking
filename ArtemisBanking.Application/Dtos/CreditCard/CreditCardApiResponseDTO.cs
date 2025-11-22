namespace ArtemisBanking.Application.Dtos.CreditCard
{
    // DTO para respuesta de GET /api/credit-card 
    public class CreditCardApiListItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string NumeroTarjeta { get; set; } = string.Empty; // 16 digitos
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public string FechaExpiracion { get; set; } = string.Empty; // MM/AA
        public decimal DeudaTotal { get; set; }
    }

    public class CreditCardApiListResponseDTO
    {
        public List<CreditCardApiListItemDTO> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // DTO para POST /api/credit-card 
    public class CreditCardApiCreateRequestDTO
    {
        public string ClienteId { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
    }

    // DTO para GET /api/credit-card/{id} 
    public class CreditCardApiDetailResponseDTO
    {
        public string TarjetaId { get; set; } = string.Empty;
        public string NumeroTarjeta { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public decimal DeudaTotal { get; set; }
        public List<CreditCardApiConsumptionDTO> Consumos { get; set; } = new();
    }

    public class CreditCardApiConsumptionDTO
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Comercio { get; set; } = string.Empty; 
        public string Estado { get; set; } = string.Empty; // APROBADO | RECHAZADO
    }

    // DTO para PATCH /api/credit-card/{id}/limit
    public class CreditCardApiUpdateLimitRequestDTO
    {
        public decimal NuevoLimite { get; set; }
    }
}

