using System.Threading.Tasks;
using ArtemisBanking.Application.ViewModels;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IClienteHomeService
    {
        Task<HomeViewModel> GetHomeDataAsync(string userId);
    }
}
