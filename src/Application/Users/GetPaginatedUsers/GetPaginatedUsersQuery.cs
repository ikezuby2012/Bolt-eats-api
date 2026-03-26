using Application.Abstractions.Messaging;
using Application.Users.Dto;
using SharedKernel;

namespace Application.Users.GetPaginatedUsers;

public sealed record GetPaginatedUsersQuery(
    int PageSize = 1000,
    int pageNumber = 1,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
    ) : IQuery<PaginatedResult<UserDto>>;
