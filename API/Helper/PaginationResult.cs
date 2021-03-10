using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Helper
{
    public class PaginationResult<T> where T: class
    {
        public PaginationResult(IEnumerable<T> data, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            Data = data;
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }

        public IEnumerable<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public static async Task<PaginationResult<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
         
            return new PaginationResult<T>(items, pageNumber, pageSize, count, (int)Math.Ceiling(count / (double)pageSize));
        }
    }
}
