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

namespace Codesearch.Work
{
    class Github
    {
        public static void Main()
        {
            Runtime runtime = new Runtime(QueueMode.Server | QueueMode.Client);

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddDebug(LogLevel.Debug)                
                .AddNesterLog(LogLevel.Debug);
            ILogger logger = loggerFactory.CreateLogger<Github>();

            GitRepository repo = new GitRepository(runtime, logger);

            var consumer = runtime.QueueServer
                .CreateConsumer();

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;
                var props = ea.BasicProperties; 

                string response = null;

                try
                {
                    if (props.Headers != null && props.Headers.ContainsKey("Type"))
                    {
                        string messageType = Encoding.UTF8.GetString(
                            props.Headers["Type"] as byte[]);

                        if (messageType == typeof(SearchQuery).ToString())
                        {
                            var message = Encoding.UTF8.GetString(body);
                            var query = JsonConvert.DeserializeObject<SearchQuery>(message);

                            SearchResult result = await repo.SearchAsync(query);

                            logger.LogInformation("Query Id {0} term {1}", 
                                query.Id, query.Text);

                            response = ResultFactory.CreateSingle<SearchResult>(
                                result, 0, "Success");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(" [.] " + e.Message);
                    response = ResultFactory.Create(
                        -1, "Failed", e.Message);
                }
                finally
                {           
                    logger.LogInformation("CorrelationId {0}", props.CorrelationId);

                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    runtime.QueueServer.DefaultChannel.Publish(
                        props.ReplyTo, responseBytes,
                        typeof(Result<SearchResult>), props.CorrelationId);
                }
            };
        }
    }
}
