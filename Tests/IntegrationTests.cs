using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using App;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    // Integration tests use the TCP/IP interface to test the server behaviour.
    public class IntegrationTests
    {
        
        private static readonly IPAddress LocalAddr = IPAddress.Parse("127.0.0.1");
        private const int Reps = 16;

        public IntegrationTests(ITestOutputHelper output)
        {
            _loggerFactory = Logging.CreateFactory(new XUnitLoggingProvider(output));
        }

        private readonly ILoggerFactory _loggerFactory;

        [Fact]
        public Task First()
        {
            int Port = 8080;
            var server = new Server(_loggerFactory);
            CancellationTokenSource source = new CancellationTokenSource();
            var task = server.Start(LocalAddr, Port, source.Token);
            var client0 = new TestClient(LocalAddr, Port, _loggerFactory);
            var client1 = new TestClient(LocalAddr, Port, _loggerFactory);
            Assert.Null(client0.EnterRoom("room"));
            Assert.Null(client1.EnterRoom("room"));
            for (var i = 0; i < Reps; ++i)
            {
                client0.WriteLine($"Hello from client0 {i}");
                client1.WriteLine($"Hello from client1 {i}");
                Assert.Equal($"[room]client-1 says 'Hello from client1 {i}'", client0.ReadLine());
                Assert.Equal($"[room]client-0 says 'Hello from client0 {i}'", client1.ReadLine());
            }
            Assert.Null(client0.LeaveRoom());
            Assert.Null(client1.LeaveRoom());
            source.Cancel();
            Assert.Equal("[Error: Server is exiting]", client1.ReadLine());
            return task;
        }

        [Fact]
        public Task ClientTriesToWriteOutsideRoom()
        {
            int Port = 8081;
            var server = new Server(_loggerFactory);
            CancellationTokenSource source = new CancellationTokenSource();
            var task = server.Start(LocalAddr, Port, source.Token);
            var client0 = new TestClient(LocalAddr, Port, _loggerFactory);
            var client1 = new TestClient(LocalAddr, Port, _loggerFactory);
            var msg = "[Error: Need to be inside a room to post a message]";
            for (var i = 0; i < Reps; ++i)
            {
                client0.WriteLine($"Hello from client0 {i}");
                client1.WriteLine($"Hello from client1 {i}");
                Assert.Equal(msg, client0.ReadLine());
                Assert.Equal(msg, client1.ReadLine());
            }
            msg = "[Error: There is no room to leave from]";
            Assert.Equal(msg, client0.LeaveRoom());
            Assert.Equal(msg, client1.LeaveRoom());
            Assert.Null(client0.Exit());
            source.Cancel();
            Assert.Equal("[Error: Server is exiting]", client1.ReadLine());
            return task;
        }

        [Fact]
        public void StartingAServerAlreadyRunning()
        {
            int Port = 8082;
            var server = new Server(_loggerFactory);
            CancellationTokenSource source = new CancellationTokenSource();
            var task = server .Start(LocalAddr, Port, source.Token);
            var msg = "Server has already started";
            for (var i = 0; i < Reps; ++i)
            {
                try
                {
                    var task2 = server .Start(LocalAddr, Port, source.Token);
                } catch (Exception e)
                {
                    Assert.Equal(msg, e.Message);
                }
                
            }
            source.Cancel();
            task.Wait();
        }
    }
}