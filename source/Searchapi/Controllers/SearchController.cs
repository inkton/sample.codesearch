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

namespace Codesearch.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly Runtime _runtime;

        public SearchController(
            ILogger<SearchController> logger,
            Runtime runtime
            )
        {
            _logger = logger;
            _runtime = runtime;
        }
        
        // GET api/search
        [HttpGet("{text}")]
        public IActionResult Get(string text)
        { 
            try
            {
                _logger.LogInformation("Query for {0} arrived", text);

                // Dispatch search jobs 

                Dictionary<string, string> queryServices = new Dictionary<string, string>();

                foreach (string service in new List<string>() { "github", "bitbucket" })
                {
                    queryServices[service] = QueryService(text, service);
                    _logger.LogInformation("Query sent to {0} -> Correlation Id {1}", 
                        service, queryServices[service]);
                }

                // ----------------------
                // Do other work here ...
                // ----------------------

                // Collect search results

               return this.NestResultMultiple(
                CollectResults(queryServices));    
            }
            catch (System.Exception e)
            {
                return StatusCode(500, e);
            }
        }

        private string QueryService(string text, string service)
        {   
            Inkton.Nest.Model.Nest searchNest = _runtime.GetNest(service);

            SearchQuery query = new SearchQuery();

            query.Id = _runtime.ComponentId + "." + DateTime.Now.Ticks.ToString();
            query.Service = service;
            query.MaxResults = 100;
            query.Text = text;

            _logger.LogInformation("Sending Query Id {0} to {1} with {2}", 
                query.Id, service, query.Text);

            _runtime.Send(query, searchNest, typeof(SearchQuery), query.Id);

            return _runtime.QueueClient.LastCorrelationId;
        }

        private List<SearchResult> CollectResults(Dictionary<string, string> queryServices)
        {
            List<SearchResult> results = new List<SearchResult>();

            int pendingResultsCount = queryServices.Count;
            DateTime start = DateTime.Now;

            while (pendingResultsCount > 0 && (DateTime.Now - start).TotalSeconds < 10 )
            {
                var returnedSingle = _runtime.ReceiveSingle<SearchResult>();

                if (returnedSingle != null)
                {
                    var result = returnedSingle.Data.Payload;
                    _logger.LogInformation("Result for Query Id -> {0}, Correlated ? {1} == {2} ",
                        result.Id, queryServices[result.Service],
                        _runtime.QueueServer.LastCorrelationId);

                    if (queryServices[result.Service] == _runtime.QueueServer.LastCorrelationId)
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
