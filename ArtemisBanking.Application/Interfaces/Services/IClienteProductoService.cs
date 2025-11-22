using ArtemisBanking.Application.ViewModels.Cliente;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IClienteProductoService
    {
        Task<DetalleCuentaViewModel?> GetDetalleCuentaAsync(string userId, int cuentaId);
        Task<DetallePrestamoViewModel?> GetDetallePrestamoAsync(string userId, int prestamoId);
        Task<DetalleTarjetaViewModel?> GetDetalleTarjetaAsync(string userId, int tarjetaId);

    }
}
