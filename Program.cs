using System.Net;
using System.Net.Sockets;
using System.Text;

var server = new Socket(SocketType.Stream, ProtocolType.Tcp);
var endPoint = IPEndPoint.Parse("10.0.0.101:13374");

server.Bind(endPoint);
server.Listen();


var manualResetEvent = new ManualResetEvent(false);
var clients = new List<Socket>();
var o = new object();

Task.Run(() =>
{
    while (true)
    {
        var client = server.Accept();
        var clientIp = ((IPEndPoint)client.RemoteEndPoint!).Address.ToString();

        clients.Add(client);
        Console.WriteLine($"{clientIp} connected!");
        Task.Run(() =>
        {
            try
            {
                var buffer = new byte[2048];

                int read;
                while ((read = client.Receive(buffer)) > 0)
                {
                    var request = Encoding.UTF8.GetString(buffer);

                    Console.WriteLine($"[{clientIp}]: {request}");

                    Array.Clear(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                lock (o)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            finally
            {
                client.Dispose();
                Console.WriteLine($"<{clientIp}> disconnected");
            }
        });
    }
});

manualResetEvent.WaitOne();