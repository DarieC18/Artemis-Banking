using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Dtos.Transaction;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface ICashierThirdPartyTransferService
    {
        Task<Result<CashierThirdPartyTransferPreviewDTO>> PreviewAsync(CashierThirdPartyTransferDTO dto);
        Task<Result<CashierThirdPartyTransferResultDTO>> ExecuteAsync(CashierThirdPartyTransferDTO dto);
    }
}
