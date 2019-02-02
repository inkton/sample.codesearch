using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Inkton.Nest.Cloud;
using Inkton.Nester;
using Inkton.Nester.Queue;
using Microsoft.EntityFrameworkCore;
using Codesearch.Model;
using Codesearch.Database;

namespace Codesearch.Work
{
    public class CacheController
    {
        private readonly QueryRepository _repo;
        private readonly Runtime _runtime;
        private readonly ILogger _logger;

        private const int TTL = 3600;

        public CacheController(
            QueryRepository repo,        
            Runtime runtime, 
            ILogger logger)
        {
            _repo = repo;
            _runtime = runtime;
            _logger = logger;
        }

        public byte[] AddQuery(byte[] data)
        {
            SearchQuery query = JsonConvert.DeserializeObject<SearchQuery>(
                    Encoding.UTF8.GetString(data));            
            SearchQuery existingQuery = _repo.GetQuery(query.Text);

            if (existingQuery == null)
            {
                _repo.AddQueryAsync(query).Wait();
            }
 
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(query));
        }

        public byte[] GetRequest(byte[] data)
        {
            SearchQuery query = _repo.GetQuery(Encoding.UTF8.GetString(data));
            List<SearchResult> cachedResults = null;

            if (query != null && (DateTime.Now - query.Created).TotalSeconds < TTL)
            {
                // If its a previous non-stale query then use the
                // results stored in the cache
                cachedResults = _repo.
                    ListResultsBySearchQuery(query.Id);
            }
            
            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(cachedResults));
        }

        public byte[] CacheResult(byte[] data)
        {
            SearchResult result = JsonConvert.DeserializeObject<SearchResult>(
                Encoding.UTF8.GetString(data));

            _repo.AddResultAsync(result).Wait();

            return Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(result));        
        }
    }
}
