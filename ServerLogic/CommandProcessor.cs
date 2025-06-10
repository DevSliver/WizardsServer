namespace WizardsServer.ServerLogic;

using System;
using System.Collections.Generic;
using System.Text;

public static class CommandProcessor
{
    public static string? ProcessCommand(string[] command, out string[] result)
    {
        if (command == null || command.Length == 0)
        {
            result = Array.Empty<string>();
            return null;
        }
        
        result = command.Length > 1 ? command[1..] : Array.Empty<string>();
        return command[0];
    }
    public static string[] SplitMessage(string message)
    {
        return message.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
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

public interface ICommandProcessor
{
    void Process(string[] args, Client client);
}