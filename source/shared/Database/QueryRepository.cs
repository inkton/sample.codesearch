using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Codesearch.Model;

namespace Codesearch.Database
{
    public class QueryRepository : IQueryRepository
    {
        private readonly QueryContext _dbContext;

        public QueryRepository (QueryContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddQueryAsync(SearchQuery query)
        {
            _dbContext.Queries.Add(query);
            await _dbContext.SaveChangesAsync();
        }

        public SearchQuery GetQuery(string text)
        {
            SearchQuery existingQuery = _dbContext.Queries
                .FirstOrDefault(query => query.Text == text);
            return existingQuery;
        }

        public async Task AddResultAsync(SearchResult result)
        {
            _dbContext.Results.Add(result);
            await _dbContext.SaveChangesAsync();
        }

        public List<SearchResult> ListResultsBySearchQuery(int searchQueryId)
        {
            var results = _dbContext.Results.Where(
                result => result.SearchQueryId == searchQueryId);
            return results.ToList();
        }
    }
}
