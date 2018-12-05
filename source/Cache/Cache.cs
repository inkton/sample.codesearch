using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Inkton.Nest.Cloud;
using Inkton.Nester;
using Inkton.Nester.Queue;
using Microsoft.EntityFrameworkCore;
using Codesearch.Database;
using Codesearch.Model;

namespace Codesearch.Work
{
    class Cache
    {    
        public static void Main()
        {
            Runtime runtime = new Runtime(
                QueueMode.Server | QueueMode.Client);

            QueryRepository repo = new QueryRepository(
                QueryContextFactory.Create(runtime));

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddDebug(LogLevel.Debug)
                .AddNesterLog(LogLevel.Debug);
            ILogger logger = loggerFactory.CreateLogger<Cache>();

            runtime.QueueServer
                .CreateRPCEndpoint(new CacheController(
                    repo, runtime, logger)).MainLoop(); 
        }        
    }    
}
