namespace ArtemisBanking.Application.Dtos.Loan
{
    // DTO para respuesta de GET /api/loan según especificación
    public class LoanApiListItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string NumeroIdentificador { get; set; } = string.Empty; // 9 dígitos
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public decimal MontoCapital { get; set; }
        public int CantidadTotalCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public bool EnMora { get; set; }
    }

    public class LoanApiListResponseDTO
    {
        public List<LoanApiListItemDTO> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }

    // DTO para POST /api/loan4
    public class LoanApiCreateRequestDTO
    {
        public string ClienteId { get; set; } = string.Empty;
        public decimal MontoPrestar { get; set; }
        public int PlazoMeses { get; set; } // 6,12,18....
        public decimal TasaInteresAnual { get; set; }
    }

    // DTO para GET /api/loan/{id} 
    public class LoanApiDetailResponseDTO
    {
        public string PrestamoId { get; set; } = string.Empty;
        public string NumeroIdentificador { get; set; } = string.Empty;
        public decimal MontoCapital { get; set; }
        public decimal TasaInteres { get; set; }
        public int PlazoMeses { get; set; }
        public List<LoanApiPaymentScheduleDTO> TablaAmortizacion { get; set; } = new();
    }

    public class LoanApiPaymentScheduleDTO
    {
        public int CuotaNumero { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal ValorCuota { get; set; }
        public bool EstadoPago { get; set; }
        public bool Atrasada { get; set; }
    }

    // DTO para PATCH /api/loan/{id}
    public class LoanApiUpdateRateRequestDTO
    {
        public decimal NuevaTasaInteres { get; set; }
    }
}

