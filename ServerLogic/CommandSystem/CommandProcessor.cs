using MessagePack;

namespace WizardsServer.ServerLogic.CommandSystem;

public class CommandProcessor
{
    private readonly Dictionary<string, Action<Session, Command>> _handlers = new();
    public Action<Session, Command> this[string key]
    {
        get => _handlers[key];
        set
        {
            if (value == null) _handlers.Remove(key);
            else _handlers[key] = value;
        }
    }

    public bool Process(Session session, Command command)
    {
        if (_handlers.TryGetValue(command.Path, out var handler))
        {
            handler(session, command);
            return true;
        }

        Console.WriteLine($"No handler registered for path '{command.Path}'");
        return false;
    }
    public static byte[] Serialize(Command command)
    {
        return MessagePackSerializer.Serialize(command);
    }
    public static Command Deserialize(byte[] command)
    {
        return MessagePackSerializer.Deserialize<Command>(command);
    }
}