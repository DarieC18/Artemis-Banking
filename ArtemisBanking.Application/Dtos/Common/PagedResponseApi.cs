namespace ArtemisBanking.Application.DTOs.Common
{
    public class PagedResponseApi<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

        public object Paginacion { get; set; } = new();
    }
}
