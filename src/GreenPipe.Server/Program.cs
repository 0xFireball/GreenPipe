using System;
using System.Net;
using System.Threading.Tasks;

namespace GreenPipe.Server {
    class Program {
        public const string version = "0.1";

        static async Task Main(string[] args) {
            Console.WriteLine($"greenpipe - server v{version}");
            var acceptor = new ClientAcceptor(new IPEndPoint(IPAddress.Any, 7004));
            await acceptor.start();
        }
    }
}