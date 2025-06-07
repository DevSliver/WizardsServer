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

        // Создание сессии для каждого нового клиента
        protected override TcpSession CreateSession() => new ClientSession(this);

        // Обработка ошибок на сервере
        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Сервер столкнулся с ошибкой: {error}");
        }
    }

    public class ClientSession : TcpSession
    {
        public ClientSession(TcpServer server) : base(server) { }

        // Обработка подключения клиента
        protected override void OnConnected()
        {
            Console.WriteLine($"Клиент с ID {Id} подключен.");
            SendClientId();
        }

        // Отправка ID клиента
        private void SendClientId()
        {
            string message = $"Ваш ID: {Id}";
            SendAsync(Encoding.UTF8.GetBytes(message));
        }

        // Обработка получения данных от клиента
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine($"Получено от клиента {Id}: {message}");
            SendClientId();
        }

        // Обработка отключения клиента
        protected override void OnDisconnected()
        {
            Console.WriteLine($"Клиент с ID {Id} отключен.");
        }

        // Обработка ошибок в сессии
        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Ошибка в сессии клиента {Id}: {error}");
        }
    }
}
