using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Inkton.Nester;
using Inkton.Nester.Queue;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Codesearch.Model;
using Codesearch.Database;
//

namespace Codesearch.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly Runtime _runtime;
        private readonly List<string> _services = new List<string>() { 
            "github", "bitbucket" };

        private const int MaxWaitSeconds = 20;        
        private NesterQueueRPCClient _cache;

        public SearchController(
            ILogger<SearchController> logger,   
            Runtime runtime
            )
        {
            _logger = logger;
            _runtime = runtime;

            _cache = _runtime.QueueClient.CreateRPCEndpoint(
                _runtime.GetNest("cache"));
            _cache.TimedOut += CacheTimedOutHandler;
        }
    
        private void CacheTimedOutHandler(object sender, EventArgs e)
        {
            _logger.LogError("Cache timeout");            
        }

        // GET api/search
        [HttpGet("{text}")]
        public IActionResult Get(string text)
        { 
            try
            {
                _logger.LogInformation("Query for {0} arrived", text);
                
                // Search the cache for query

                List<SearchResult> cachedResults = GetCachedResults(text);

                if (cachedResults != null && cachedResults.Count > 0)
                {
                    return this.NestResultMultiple(cachedResults, 0, 
                        "Taken from cache");  
                }

                // Dispatch search jobs with a new search

                Dictionary<string, SearchQuery> queryServices = 
                    new Dictionary<string, SearchQuery>();

                SearchQuery query = new SearchQuery();
                query.MaxResults = 100;
                query.Text = text;

                query = AddQuery(text);

                foreach (string service in _services)
                {
                    queryServices[service] = QueryService(query, service);
                    _logger.LogInformation("Query sent to {0} -> Correlation Id {1}", 
                        service, queryServices[service].Id);
                }

                // ----------------------
                // Do other work here ...
                // ----------------------

                // Collect search results
 
               return this.NestResultMultiple(
                CollectResults(queryServices), 0, 
                    "Query successful");    
            }
            catch (System.Exception e)
            {
                return StatusCode(500, e);
            }
        }

        private List<SearchResult> GetCachedResults(string text)
        {
            List<SearchResult> result = null;

            byte[] rawResult = _cache.Call("GetRequest", Encoding.UTF8.GetBytes(text));

            if (rawResult != null)
            {
                result = JsonConvert.DeserializeObject<List<SearchResult>>(
                                Encoding.UTF8.GetString(rawResult));
            }

            return result;
        }

        private SearchQuery AddQuery(string text)
        {
            SearchQuery query = new SearchQuery();
            query.MaxResults = 100;
            query.Text = text;

            byte[] result = _cache.Call("AddQuery", Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(query)));

            if (result != null)
            {
                query = JsonConvert.DeserializeObject<SearchQuery>(
                    Encoding.UTF8.GetString(result));
            }
            
            return query;
        }

        private SearchQuery QueryService(SearchQuery query, string service)
        {   
            Inkton.Nest.Model.Nest searchNest = _runtime.GetNest(service);

            _logger.LogInformation("Sending Query Id {0} to {1} with {2}", 
                query.Id, service, query.Text);

            _runtime.Send(query, searchNest, typeof(SearchQuery), query.Id.ToString());

            return query;
        }

        private List<SearchResult> CollectResults(Dictionary<string, SearchQuery> queryServices)
        {
            List<SearchResult> results = new List<SearchResult>();

            int pendingResultsCount = queryServices.Count;
            DateTime start = DateTime.Now;

            while (pendingResultsCount > 0 &&
                (DateTime.Now - start).TotalSeconds < MaxWaitSeconds)
            {
                var returnedSingle = _runtime.ReceiveSingle<SearchResult>();

                if (returnedSingle != null)
                {
                    var result = returnedSingle.Data.Payload;
                    _logger.LogInformation("Result for Query Id -> {0}, Correlated ? {1} == {2} ",
                        result.Id, queryServices[result.Service].Id,
                        _runtime.QueueServer.LastCorrelationId);

                    if (queryServices[result.Service].Id.ToString() 
                        == _runtime.QueueServer.LastCorrelationId)
                    {
                        results.Add(result);
                        --pendingResultsCount;
                    }
                }
            }

            return results;
        }        
    }
}
