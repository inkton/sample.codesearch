using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Inkton.Nester;
using Inkton.Nester.Queue;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Codesearch.Model;
using Codesearch.Controllers;
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

            mockRuntime.Setup(x => x.ReceiveSingle<SearchResult>())
	        	.Returns(ResultSingle<SearchResult>.ConvertObject(
                    GetInput("test-input-a"), new SearchResult()));
            mockRuntime.Setup(x => x.ReceiveSingle<SearchResult>())
	        	.Returns(ResultSingle<SearchResult>.ConvertObject(
                    GetInput("test-input-b"), new SearchResult()));

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<SearchController>();

            var controller = new SearchController(
                logger, mockRuntime.Object);

            // Act
            var result = controller.Get("docker");

            // Assert
            var okResult = result as JsonResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<Result<List<SearchResult>>>(okResult.Value);
        }

        private string GetInput(string name)
        {
            string content;

            using (FileStream fs = File.Open("data/" + name + ".json", FileMode.Open)) 
            {
                using (var reader = new StreamReader(fs))
                {
                    content = reader.ReadToEnd();
                }
            }

            return content;
        }
    }
}
