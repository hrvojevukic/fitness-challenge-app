using FitnessChallenge.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Services;

internal static class PagingExtensions
{
    public static async Task<PagedResult<TResponse>> ToPagedResultAsync<TEntity, TResponse>(
        this IQueryable<TEntity> query,
        PageQuery paging,
        Func<TEntity, TResponse> map)
    {
        var totalCount = await query.CountAsync();
        var entities = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync();

        var items = new List<TResponse>();
        foreach (var entity in entities)
        {
            items.Add(map(entity));
        }

        return new PagedResult<TResponse>
        {
            Items = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = totalCount,
        };
    }
}
