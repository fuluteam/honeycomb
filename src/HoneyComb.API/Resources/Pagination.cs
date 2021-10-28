


using System;
using System.Collections.Generic;

namespace HoneyComb.API.Resources
{
    public class PaginatedList<T>
    {
        public PaginatedList(List<T> items, long count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            Items = items;
        }

        public int PageIndex { get; }
        public int TotalPages { get; }
        public long TotalCount { get; }
        public List<T> Items { get; }

        public bool HasPreviousPage => (PageIndex > 1);

        public bool HasNextPage => (PageIndex < TotalPages);

        //public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        //{
        //    var count = await source.CountAsync();
        //    var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        //    return new PaginatedList<T>(items, count, pageIndex, pageSize);
        //}
    }
}
