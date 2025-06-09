namespace WizardsServer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CommandProcessor
{
    private static CommandProcessor _global = new CommandProcessor();
    public static CommandProcessor Global => _global;

    private readonly Dictionary<string, Action<string[], Client>> _handlers = new();

    public void Subscribe(string command, Action<string[], Client> handler)
    {
        if (!_handlers.ContainsKey(command))
            _handlers[command] = delegate { };

        _handlers[command] += handler;
    }

    public void Unsubscribe(string command, Action<string[], Client> handler)
    {
        if (_handlers.ContainsKey(command))
            _handlers[command] -= handler;
    }
    public void ClearAllSubscriptions()
    {
        _handlers.Clear();
    }
    public void ProcessCommand(string[] commandArray, Client client)
    {
        if (commandArray == null || commandArray.Length == 0)
            return;

        string[] args = commandArray.Length > 1 ? commandArray[1..] : Array.Empty<string>();

        if (_handlers.TryGetValue(commandArray[0], out var handlers))
        {
            handlers.Invoke(args, client);
        }
    }
    public static string[] SplitCommandLine(string commandLine)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        bool insideBackticks = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (c == '`')
            {
                insideBackticks = !insideBackticks;
                continue;
            }

            if (char.IsWhiteSpace(c) && !insideBackticks)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            args.Add(current.ToString());
        }

        return args.ToArray();
    }
}
