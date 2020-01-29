using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    private List<ServerClient> clients;
    private List<ServerClient> disconnectedClients;
    private TcpListener server;
    private bool serverStarted;

    public int port = 6321;
    private int playerId = 0;

    private void Awake()
    {
        clients = new List<ServerClient>();
        disconnectedClients = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log($"Server started on {port}");
        }
        catch
        {
            Debug.Log("Server already running");
        }
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (var client in clients)
        {
            // Is connected?
            if (!IsConnected(client.tcp))
            {
                client.tcp.Close();
                disconnectedClients.Add(client);
                continue;
            }
            else
            {
                NetworkStream stream = client.tcp.GetStream();
                if (stream.DataAvailable)
                {
                    StreamReader reader = new StreamReader(stream);
                    string data = reader.ReadLine();
                    if (!string.IsNullOrEmpty(data))
                    {
                        OnIncomingData(client, data);
                    }
                }
            }
        }
    }

    private void OnIncomingData(ServerClient client, string data)
    {
        BroadCast(data, clients.Where(c => c.playerId != client.playerId).ToList());
    }

    private void BroadCast(string data, List<ServerClient> clients)
    {
        foreach (var client in clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch
            {
                Debug.Log("Write error");
            }
        }
    }

    private bool IsConnected(TcpClient tcp)
    {
        return tcp != null && tcp.Client != null && tcp.Client.Connected;
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        if (clients.Count >= 2)
        {
            print("Too many players!");
            return;
        }
        TcpListener listener = (TcpListener)ar.AsyncState;

        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar), playerId++));
        StartListening();

        // Send message to everyone
        // Move 'other player'
        print("Player connected.");
    }
}

public class ServerClient
{
    public readonly TcpClient tcp;
    public readonly int playerId;

    public ServerClient(TcpClient tcp, int playerId)
    {
        this.tcp = tcp;
        this.playerId = playerId;
    }
}