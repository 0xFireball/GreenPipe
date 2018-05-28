using System;
using System.Net.Sockets;

namespace GreenPipe.Server {
    public class ClientInfo {
        public TcpClient conn { get; }
        public string id { get; } = Guid.NewGuid().ToString();
        public ClientInfo peer { get; set; }
        public byte[] buffer { get; } = new byte[8192];

        public ClientInfo(TcpClient conn) {
            this.conn = conn;
        }
    }
}