using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    private readonly bool socketReady;
    private StreamReader reader;
    private StreamWriter writer;
    private TcpClient socket;
    private NetworkStream stream;

    private static Client client;

    public static Client GetInstance()
    {
        client = client ?? new Client();
        return client;
    }

    private Client()
    {
        if (socketReady)
        {
            return;
        }
        try
        {
            string host = "127.0.0.1";
            int port = 6321;

            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    public object CheckMessages()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (!string.IsNullOrEmpty(data))
                {
                    return OnIncomingData(data);
                }
            }
        }
        return null;
    }


    public void SendMovement(Vector3 targetPosition, Quaternion playerRot, Vector3 currentPosition)
    {
        if (socketReady)
        {
            PlayerMovement movementData = new PlayerMovement(targetPosition, playerRot, currentPosition);
            Send(JsonUtility.ToJson(movementData));
        }
    }

    public void SendBall(Vector3 targetPosition)
    {
        if (socketReady)
        {
            BallMovement sendBall = new BallMovement(targetPosition);
            Send(JsonUtility.ToJson(sendBall));
        }
    }

    private void Send(string data)
    {
        writer.WriteLine(data);
        writer.Flush();
    }

    private object OnIncomingData(string data)
    {
        Debug.Log("Other player moved.");
        if (data.Contains("playerRot"))
        {
            return JsonUtility.FromJson<PlayerMovement>(data);
        } 
        else if (data.Contains("ball"))
        {
            return JsonUtility.FromJson<BallMovement>(data);
        }
        else return null;
    }
}

