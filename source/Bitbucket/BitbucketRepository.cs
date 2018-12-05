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
    public class BitbucketRepository
    {
        private Runtime _runtime;
        private ILogger _logger;
        private NesterQueueRPCClient _cache;

        public BitbucketRepository(Runtime runtime, ILogger logger)
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
            string baseUrl = "https://api.bitbucket.org/2.0/teams/%7B6f461d1e-a3dd-433b-a0e3-7a69daf6ea47%7D/search/code?search_query="+ 
                    HttpUtility.UrlEncode(query.Text);

            SearchResult result = new SearchResult();

            using (HttpClient client = new HttpClient())
            {
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
                            result.Service = "bitbucket";
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

            return JsonConvert.DeserializeObject<SearchResult>(
                                Encoding.UTF8.GetString(result));
        }        
    }
}
