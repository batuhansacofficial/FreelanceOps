namespace FreelanceOps.Application.Common.Pagination;

public sealed record PaginationQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null);
