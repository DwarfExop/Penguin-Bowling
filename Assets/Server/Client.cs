using Assets.Enums;
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
    private Vector3 lastPosition;

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

    public (MovementData data, MovementType? type) CheckMessages()
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
        return (null, null);
    }

    public void SendMovement(GameObject other, Vector3 targetPosition, Quaternion playerRot, Vector3 currentPosition)
    {
        if (socketReady)
        {
            var movementData = new MovementData(targetPosition, playerRot, currentPosition);
            Send(MovementType.Player, JsonUtility.ToJson(movementData));
        }
    }

    public void SendBall(GameObject other, Vector3 targetPosition, Quaternion playerRot, Vector3 currentPosition)
    {
        if (socketReady)
        {
            var movementData = new MovementData(targetPosition, playerRot, currentPosition);
            Send(MovementType.Ball, JsonUtility.ToJson(movementData));
        }
    }

    private void Send(MovementType action, string data)
    {
        writer.WriteLine(action.ToString() + data);
        writer.Flush();
    }

    private (MovementData data, MovementType? type) OnIncomingData(string data)
    {
        Debug.Log("Other player moved.");
        if (data.StartsWith(MovementType.Player.ToString()))
        {
            return (JsonUtility.FromJson<MovementData>(data.Replace(MovementType.Player.ToString(), "")), MovementType.Player);
        }
        else if (data.StartsWith(MovementType.Ball.ToString()))
        {
            return (JsonUtility.FromJson<MovementData>(data.Replace(MovementType.Ball.ToString(), "")), MovementType.Ball);
        }
        else return (null, null);
    }
}

public class MovementData
{
    public Vector3 targetPosition;
    public Quaternion playerRot;
    public Vector3 currentPosition;

    public MovementData(Vector3 targetPosition, Quaternion playerRot, Vector3 currentPosition)
    {
        this.targetPosition = targetPosition;
        this.playerRot = playerRot;
        this.currentPosition = currentPosition;
    }
}