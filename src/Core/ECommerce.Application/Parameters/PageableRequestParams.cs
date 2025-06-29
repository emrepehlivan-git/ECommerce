namespace ECommerce.Application.Parameters;

public record PageableRequestParams(int Page = 1, int PageSize = 10, string? Search = null);

