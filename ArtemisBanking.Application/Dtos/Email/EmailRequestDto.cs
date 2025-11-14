using System.Collections.Generic;

namespace ArtemisBanking.Application.DTOs.Email
{
    public class EmailRequestDto
    {
        public string? To { get; set; }
        public IList<string>? ToRange { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}
