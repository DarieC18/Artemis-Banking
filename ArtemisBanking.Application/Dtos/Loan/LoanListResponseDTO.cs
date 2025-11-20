namespace ArtemisBanking.Application.Dtos.Loan
{
    public class LoanListResponseDTO
    {
        public List<LoanListItemDTO> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}

