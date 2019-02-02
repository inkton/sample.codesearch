using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Net.Http; 
using System.Net.Http.Headers;
using System.Web;
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

namespace Codesearch.Work
{
    public class GitRepository
    {
        private Runtime _runtime;
        private ILogger _logger;
        private NesterQueueRPCClient _cache;

        public GitRepository(Runtime runtime, ILogger logger)
        {
            _runtime = runtime;
            _logger = logger;

            _cache = _runtime.QueueClient.CreateRPCEndpoint(
                _runtime.GetNest("cache"));
            _cache.TimedOut += CacheTimedOutHandler;            
        }

        private void CacheTimedOutHandler(object sender, EventArgs e)
        {
            _logger.LogError("Cache timeout");
        }

        public async Task<SearchResult> SearchAsync(SearchQuery query)
        {            
            string baseUrl = "https://api.github.com/search/repositories?q="+ 
                    HttpUtility.UrlEncode(query.Text) + "&sort=stars&order=desc";

            SearchResult result = new SearchResult();

            using (HttpClient client = new HttpClient())
            {
                // https://developer.github.com/v3/#user-agent-required
                client.DefaultRequestHeaders.Add("User-Agent", "nest-don");
                
                //Setting up the response...
                using (HttpResponseMessage res = await client.GetAsync(baseUrl))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        if (data != null)
                        {
                            result.SearchQueryId = query.Id;
                            result.Service = "github";
                            result.HandledBy = _runtime.ComponentId;
                            result.Data = data;

                            return CacheResult(result);
                        }
                    }
                }
            }
            
            return null;
        }

        private SearchResult CacheResult(SearchResult searchResult)
        {
            byte[] result = _cache.Call("CacheResult", Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(searchResult)));
            
            if (result != null)
            {
                // The cache allocates and returns an id for the cached result
                searchResult = JsonConvert.DeserializeObject<SearchResult>(
                                    Encoding.UTF8.GetString(result));
            }

            return searchResult;
        }          
    }  
}
