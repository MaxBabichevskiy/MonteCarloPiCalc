using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

class Server
{
    static ConcurrentBag<(double x, double y)> allPoints = new ConcurrentBag<(double x, double y)>();
    static object lockObj = new object();
    static int totalPoints = 0;
    static int pointsInsideCircle = 0;

    static async Task Main(string[] args)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 12345;

        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();
        Console.WriteLine("Server started. Waiting for clients...");

        Task aggregationTask = Task.Run(AggregatePoints);

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        if (receivedData.StartsWith("POINTS"))
        {
            int pointsCount = int.Parse(receivedData.Substring(7));
            lock (lockObj)
            {
                totalPoints += pointsCount;
            }

            for (int i = 0; i < pointsCount; i++)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                double x = BitConverter.ToDouble(buffer, 0);
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                double y = BitConverter.ToDouble(buffer, 0);

                allPoints.Add((x, y));
            }

            Console.WriteLine($"Received {pointsCount} points from a client.");
        }

        client.Close();
    }

    static async Task AggregatePoints()
    {
        while (true)
        {
            await Task.Delay(1000);

            int pointsInside = 0;

            foreach ((double x, double y) in allPoints)
            {
                if (x * x + y * y <= 1)
                {
                    pointsInside++;
                }
            }

            lock (lockObj)
            {
                pointsInsideCircle += pointsInside;
                double calculatedPi = 4.0 * pointsInsideCircle / totalPoints;
                Console.WriteLine($"Current estimation of π: {calculatedPi}");
            }
        }
    }
}
