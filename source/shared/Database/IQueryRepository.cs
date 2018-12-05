using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Codesearch.Model;

namespace Codesearch.Database
{    
    public interface IQueryRepository
    {
        Task AddQueryAsync(SearchQuery query);
        SearchQuery GetQuery(string text);

        Task AddResultAsync(SearchResult result);
        List<SearchResult> ListResultsBySearchQuery(int searchQueryId);
    }
}
