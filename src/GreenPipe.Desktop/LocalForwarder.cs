﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GreenPipe.Desktop {
    public class LocalForwarder {
        private TcpListener _listener;
        public TcpClient serverConn;

        public LocalForwarder(IPEndPoint endpoint) {
            _listener = new TcpListener(endpoint);
            serverConn = new TcpClient();
        }

        public async Task runListener() {
            while (true) {
                var extClient = await _listener.AcceptTcpClientAsync();

                var extInfo = new ConnInfo(extClient);
                var serverConnInfo = new ConnInfo(serverConn);
                serverConnInfo.peer = extInfo;
                extInfo.peer = serverConnInfo;

                // forward
                serverConnInfo.conn.Client.BeginReceive(serverConnInfo.buffer, 0, serverConnInfo.buffer.Length,
                    SocketFlags.None,
                    onClientReceieveAsync, serverConnInfo);
                extInfo.conn.Client.BeginReceive(extInfo.buffer, 0, extInfo.buffer.Length, SocketFlags.None,
                    onClientReceieveAsync, extInfo);
            }
        }

        private void onClientReceieveAsync(IAsyncResult ar) {
            var connInfo = (ConnInfo) ar.AsyncState;
            try {
                var read = connInfo.conn.Client.EndReceive(ar);
                if (read > 0) {
                    // send to peer
                    connInfo.peer.conn.Client.Send(connInfo.buffer);
                    // continue receiving
                    connInfo.conn.Client.BeginReceive(connInfo.buffer, 0, connInfo.buffer.Length,
                        SocketFlags.None, onClientReceieveAsync, null);
                }
            } finally {
                connInfo.conn.Close();
                connInfo.peer.conn.Close();
            }
        }

        class ConnInfo {
            public TcpClient conn { get; set; }
            public ConnInfo peer { get; set; }
            public byte[] buffer = new byte[8192];

            public ConnInfo(TcpClient conn) {
                this.conn = conn;
            }
        }
    }
}