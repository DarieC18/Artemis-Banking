using System;

namespace ArtemisBanking.Application.Dtos.SavingsAccount
{
    public class CreateSavingsAccountDTO
    {
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool IsActive { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
