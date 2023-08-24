using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main(string[] args)
    {
        string serverIp = "127.0.0.1";
        int serverPort = 12345;
        int pointsCount = 10000;

        TcpClient client = new TcpClient();
        client.Connect(serverIp, serverPort);

        NetworkStream stream = client.GetStream();

        string pointsMessage = $"POINTS{pointsCount}";
        byte[] pointsMessageBytes = Encoding.ASCII.GetBytes(pointsMessage);
        stream.Write(pointsMessageBytes, 0, pointsMessageBytes.Length);

        Random random = new Random();
        for (int i = 0; i < pointsCount; i++)
        {
            double x = random.NextDouble();
            double y = random.NextDouble();

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] yBytes = BitConverter.GetBytes(y);

            stream.Write(xBytes, 0, xBytes.Length);
            stream.Write(yBytes, 0, yBytes.Length);
        }

        client.Close();
    }
}
