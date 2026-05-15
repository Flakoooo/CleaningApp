namespace CleaningAppWeb.Domain.DTOs
{
    public record class ListDataResponse<T>(int Count, IReadOnlyCollection<T> List);
}
