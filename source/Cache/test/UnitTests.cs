using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Inkton.Nester;
using Inkton.Nester.Queue;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Codesearch.Model;
using Codesearch.Work;
using Codesearch.Database;
using Xunit;
using Moq;

namespace Codesearch.Tests
{
    public class UnitTests
    {
        [Fact]
        public void TestSearch()
        {
             // Arrange
            var mockRuntime = new Mock<Runtime>(
                QueueMode.Server | QueueMode.Client,
                180, Enviorenment.Development
            ) {CallBase = true};
        
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<CacheController>();

            QueryRepository repo = new QueryRepository(
                QueryContextFactory.Create(mockRuntime.Object));

            CacheController cache = new CacheController(
                repo, mockRuntime.Object, logger);

            // Act
            SearchQuery query = new SearchQuery{ 
                Text = "docker",
                MaxResults = 100
            };

            byte[] rawQuery = cache.AddQuery(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(query)));

            SearchQuery cachedQuery = JsonConvert.DeserializeObject<SearchQuery>(
                Encoding.UTF8.GetString(rawQuery));

            SearchResult result = new SearchResult();
            result.SearchQueryId = cachedQuery.Id;
            result.Service = "bitbucket";
            result.HandledBy = "cacheUnitTest";
            result.Data = "{'result': 'Hello World'}";

            byte[] rawQueryResult = cache.CacheResult(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(result)));

            SearchResult cachedResult = JsonConvert.DeserializeObject<SearchResult>(
                Encoding.UTF8.GetString(rawQueryResult));

            // Assert
            Assert.NotNull(cachedResult);
            Assert.True(cachedResult.Data == result.Data);
        }
    }
}
