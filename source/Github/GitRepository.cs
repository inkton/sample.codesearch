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
        ILogger _logger;

        public GitRepository(Runtime runtime, ILogger logger)
        {
            _runtime = runtime;
            _logger = logger;
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
                            result.Id = query.Id;
                            result.Service = query.Service;
                            result.HandledBy = _runtime.ComponentId;
                            result.Data = data;
                            return result;
                        }
                    }
                }
            }
            
            return null;
        }
    }
}
