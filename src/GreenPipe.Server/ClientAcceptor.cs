﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GreenPipe.Server {
    public class ClientAcceptor : IDisposable {
        private TcpListener _listener;
        private bool _running;
        private List<ClientInfo> _clients = new List<ClientInfo>();

        public ClientAcceptor(IPEndPoint endpoint) {
            _listener = new TcpListener(endpoint);
        }

        public async Task start() {
            _running = true;
            _listener.Start();
            Console.WriteLine($"listening at {_listener.LocalEndpoint}");
            while (_running) {
                var acceptClientTask = _listener.AcceptTcpClientAsync();
                var clientSock = await acceptClientTask;
                var client = new ClientInfo(clientSock);
                _clients.Add(client);
                Console.WriteLine($"accepted client {client.id}");
                try {
                    var clientLoopTask = runClientLoop(client);
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }
        }

        private async Task<bool> runClientLoop(ClientInfo client) {
            var clientStream = client.conn.GetStream();
            var writer = new StreamWriter(clientStream);
            var reader = new StreamReader(clientStream);
            // send ID to client
            writer.Write(client.id + "\n");
            writer.Flush();
            // wait for peer information
            while (true) {
                var peerId = reader.ReadLine();
                if (peerId == null) continue;
                var peer = _clients.FirstOrDefault(x => x.id == peerId);
                if (peer == null) {
                    if (client.peer == null) {
                        // peer not found
                        Console.WriteLine($"closed connection with {client.id}");
                        client.conn.Close();
                        return false;
                    }

                    // if client.peer is NOT null, the other client connected to us already
                    peer = client.peer;
                } else {
                    client.peer = peer; // my peer is them
                    peer.peer = client; // their peer is me
                }

                break;
            }

            // set up forwarding between peers
            client.conn.Client.BeginReceive(client.buffer, 0, client.buffer.Length, SocketFlags.None,
                onClientReceieveAsync, client);

            return true;
        }

        private void onClientReceieveAsync(IAsyncResult ar) {
            var clientInfo = (ClientInfo) ar.AsyncState;
            try {
                var read = clientInfo.conn.Client.EndReceive(ar);
                if (read > 0) {
                    // send to peer
                    clientInfo.peer.conn.Client.Send(clientInfo.buffer);
                    // continue receiving
                    clientInfo.conn.Client.BeginReceive(clientInfo.buffer, 0, clientInfo.buffer.Length,
                        SocketFlags.None, onClientReceieveAsync, null);
                }
            } finally {
                clientInfo.conn.Close();
                clientInfo.peer.conn.Close();
            }
        }

        public void Dispose() {
            _listener.Server.Dispose();
        }
    }
}