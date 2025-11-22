namespace ArtemisBanking.Application.Dtos.CreditCard
{
    public class CreditCardListResponseDTO
    {
        public List<CreditCardListItemDTO> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}

