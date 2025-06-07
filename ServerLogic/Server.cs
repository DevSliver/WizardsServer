using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace WizardsServer
{
    public class Server : TcpServer
    {
        public Server(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession()
        {
            return new Client(this);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Сервер столкнулся с ошибкой: {error}");
        }
    }

    public class Client : TcpSession
    {
        public Client(TcpServer server) : base(server) { }

        protected override void OnConnected()
        {
            Console.WriteLine($"Клиент с ID {Id} подключен.");
        }
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine($"Получено от клиента {Id}: {message}");

            var context = new ConnectionContext(this);
            CommandProcessor.Instance.ProcessCommand(message, context);
        }
        protected override void OnDisconnected()
        {
            Console.WriteLine($"Клиент с ID {Id} отключен.");
        }
        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Ошибка в сессии клиента {Id}: {error}");
        }
    }
    public class ConnectionContext : IConnectionContext
    {
        private readonly Client _client;

        public ConnectionContext(Client client)
        {
            _client = client;
        }

        public void SendAsync(string message)
        {
            _client.SendAsync(message);
        }
    }
}
