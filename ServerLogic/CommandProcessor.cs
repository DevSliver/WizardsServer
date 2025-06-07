using System;
using System.Collections.Generic;
using System.Net.Sockets;
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

            string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string command = parts[0].ToLowerInvariant();
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (_handlers.TryGetValue(command, out var handlers))
            {
                handlers.Invoke(args, context);
            }
        }
    }
}
