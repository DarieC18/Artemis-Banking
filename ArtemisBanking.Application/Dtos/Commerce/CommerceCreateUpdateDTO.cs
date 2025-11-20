namespace ArtemisBanking.Application.DTOs.Commerce
{
    public class CommerceCreateUpdateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Logo { get; set; }
    }
}
