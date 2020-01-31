using System.Net.Sockets;

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