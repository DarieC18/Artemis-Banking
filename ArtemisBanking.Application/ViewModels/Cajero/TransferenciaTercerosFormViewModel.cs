using System.ComponentModel.DataAnnotations;

public class CashierThirdPartyTransferViewModel
{
    [Required(ErrorMessage = "La cuenta origen es obligatoria.")]
    [Display(Name = "Cuenta origen")]
    public string CuentaOrigen { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cuenta destino es obligatoria.")]
    [Display(Name = "Cuenta destino")]
    public string CuentaDestino { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
    [Display(Name = "Monto a transferir")]
    public decimal Monto { get; set; }
}
