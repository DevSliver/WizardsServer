namespace WizardsServer;

using System;
using System.Collections.Generic;
using System.Text;

public class CommandProcessor
{
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

    public void ProcessCommand(string commandLine, Client client)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return;

        var parts = SplitCommandLine(commandLine);

        if (parts.Count == 0)
            return;

        string command = parts[0].ToLowerInvariant();
        string[] args = parts.Count > 1 ? parts.GetRange(1, parts.Count - 1).ToArray() : Array.Empty<string>();

        if (_handlers.TryGetValue(command, out var handlers))
        {
            handlers.Invoke(args, client);
        }
    }
    public static List<string> SplitCommandLine(string commandLine)
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

        return args;
    }
}
