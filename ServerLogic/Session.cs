using NetCoreServer;
using System.Diagnostics;
using System.Text;

namespace WizardsServer.ServerLogic;

public class Session : TcpSession
{
    private Server _server;
    public Client Client => _server.Clients[this];

    public Session(Server server) : base(server)
    {
        _server = server;
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        var commands = CommandProcessor.SplitMessage(message);
        foreach (var command in commands)
        {
            Console.WriteLine($"Команда от сервера: {command}");
            var args = CommandProcessor.SplitCommandLine(command);
            if (args.Length == 0)
                continue;
            Client.ReceiveCommand(long.Parse(args[0]), args[1..]);
        }
    }
    protected override void OnError(System.Net.Sockets.SocketError error)
    {
        _server.OnSessionError(this, error);
    }
}