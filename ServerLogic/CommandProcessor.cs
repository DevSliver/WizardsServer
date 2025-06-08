using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace WizardsServer
{
    public interface IConnectionContext
    {
        void SendAsync(string message);
    }

    public class CommandProcessor
    {
        public static CommandProcessor Instance { get; } = new CommandProcessor();

        private readonly Dictionary<string, Action<string[], IConnectionContext>> _handlers = new();

        private CommandProcessor() { }

        public void Subscribe(string command, Action<string[], IConnectionContext> handler)
        {
            if (!_handlers.ContainsKey(command))
                _handlers[command] = delegate { };

            _handlers[command] += handler;
        }

        public void Unsubscribe(string command, Action<string[], IConnectionContext> handler)
        {
            if (_handlers.ContainsKey(command))
                _handlers[command] -= handler;
        }

        public void ProcessCommand(string commandLine, IConnectionContext context)
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
                handlers.Invoke(args, context);
            }
        }

        private List<string> SplitCommandLine(string commandLine)
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
}
