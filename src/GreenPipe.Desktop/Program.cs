using System;
using System.Net;
using System.Net.Sockets;

namespace GreenPipe.Desktop {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("greenpipe console");
            var forwarder = new LocalForwarder(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5004));
            if (args.Length > 0 && args[0] == "i") {
                // use interactive mode
                while (true) {
                    var cm = Console.ReadLine();
                    if (cm == "q") {
                        break;
                    } else if (cm.StartsWith("c ")) {
                        var destStr = cm.Substring(2);
                        var destIpeParts = destStr.Split(':');
                        var destAddr = IPAddress.Parse(destIpeParts[0]);
                        var destPort = int.Parse(destIpeParts[1]);
                        var destEp = new IPEndPoint(destAddr, destPort);
                        Console.Write($"connecting to {destEp} ...");
                        forwarder.serverConn.Connect(destEp);
                        Console.WriteLine("done.");
                    } else if (cm == "s") {
                        // start forwarder
                        var forwarderRunTask = forwarder.runListener();
                    } else {
                        Console.WriteLine($"unrecognized command {cm}");
                    }
                }
            }
        }
    }
}