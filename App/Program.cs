using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace App
{
    public class Program
    {
        async static Task Main()
        {


            var loggerFactory = Logging.CreateFactory();
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Starting program");

            var server = new Server(loggerFactory);
            var port = 8080;
            var localAddr = IPAddress.Parse("127.0.0.1");

            CancellationTokenSource source = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                logger.LogInformation("Stopping the server");
                source.Cancel();
            };

            await server.Start(localAddr, port, source.Token);
        }
    }
}