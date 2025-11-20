namespace ArtemisBanking.Application.DTOs.Commerce
{
    public class CommerceListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Logo { get; set; }
    }
}
