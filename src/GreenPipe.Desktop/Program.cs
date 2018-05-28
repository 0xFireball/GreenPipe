using System;
using System.Net;
using System.Threading.Tasks;

namespace GreenPipe.Desktop {
    class Program {
        static async Task Main(string[] args) {
            Console.WriteLine("greenpipe console");
            var forwarder = new LocalForwarder(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5004));
            if (args.Length > 0 && args[0] == "i") {
                // use interactive mode
                while (true) {
                    Console.Write("gp> ");
                    var cm = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(cm)) continue;
                    if (cm == "q") {
                        break;
                    } else if (cm.StartsWith("c ")) {
                        var destStr = cm.Substring(2);
                        var destIpeParts = destStr.Split(':');
                        var destAddr = IPAddress.Parse(destIpeParts[0]);
                        var destPort = int.Parse(destIpeParts[1]);
                        var destEp = new IPEndPoint(destAddr, destPort);
                        Console.WriteLine($"connecting to {destEp}.");
                        await forwarder.connectToServer(destEp);
                    } else if (cm == "s") {
                        // start forwarder
                        Console.WriteLine("starting listener");
                        var forwarderRunTask = forwarder.runListener();
                    } else if (cm.StartsWith("p ")) {
                        var peerId = cm.Substring(2);
                        Console.WriteLine($"connecting to {peerId}...");
                        var result = await forwarder.connectRemotePeer(peerId);
                    } else {
                        Console.WriteLine($"unrecognized command `{cm}`");
                    }
                }
            }
        }
    }
}