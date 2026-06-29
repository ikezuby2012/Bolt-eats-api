using Domain.MenuItem;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Helpers;

#pragma warning disable CA1304 // Specify CultureInfo
#pragma warning disable CA1311 // Specify a culture or use an invariant version
public static class KeywordPredicateBuilder
{

    public static IQueryable<MenuItem> WhereMatchesAnyKeyword(
        IQueryable<MenuItem> source,
        string[] keywords)
    {
        IQueryable<MenuItem>? result = null;

        foreach (string keyword in keywords)
        {
            string pattern = $"%{keyword}%".ToLower();
            IQueryable<MenuItem> slice = source.Where(m =>
                EF.Functions.Like(m.Category.Name.ToLower(), pattern) ||
                EF.Functions.Like(m.Name.ToLower(), pattern));

            result = result is null ? slice : result.Union(slice);
        }

        return result ?? source.Where(_ => false);
    }
}
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore CA1304 // Specify CultureInfo
