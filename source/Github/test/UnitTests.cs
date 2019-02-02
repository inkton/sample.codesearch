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
using Xunit;
using Moq;

namespace Codesearch.Tests
{
    public class UnitTests
    {
        [Fact]
        public async void TestSearchAsync()
        {
            /* 
                Perform a search and return results 
            */

             // Arrange
            var runtime = new Runtime(
                QueueMode.Server | QueueMode.Client,
                180, Enviorenment.Development
            );
        
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<GitRepository>();

            GitRepository repo = new GitRepository(
                runtime, logger);

            // Act
            SearchQuery query = new SearchQuery{ 
                Text = "docker",
                MaxResults = 100
            };

            SearchResult result = await repo.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
        }
    }
}
