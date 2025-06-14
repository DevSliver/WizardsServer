using MessagePack;

namespace WizardsServer.ServerLogic.CommandSystem;

[MessagePackObject]
public class Command
{
    [Key(0)]
    public readonly Guid Id;
    [Key(1)]
    public readonly string Path;
    [Key(2)]
    public readonly Args Args;

    [SerializationConstructor]
    public Command(Guid id, string path, Args args)
    {
        Id = id;
        Path = RemoveTrailingSlash(path);
        Args = args;
    }
    public Command(string path)
    {
        Id = Guid.NewGuid();
        Path = RemoveTrailingSlash(path);
        Args = new Args();
    }
    public static string RemoveTrailingSlash(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.TrimEnd('.');
    }
    public static string RemoveLastSegment(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        path = path.TrimEnd('.');
        int lastSlashIndex = path.LastIndexOf('.');
        if (lastSlashIndex < 0)
            return string.Empty;

        return path.Substring(0, lastSlashIndex);
    }
    public static Command MsgResponse(Command command, string message)
    {
        Command response = new Command("Response.Command");
        response.Args.Add("Message", message).Add("Id", command.Id);
        return response;
    }
    public static Command Response(Command command, Args args)
    {
        Command response = new Command("Response.Command");
        response.Args.Add("Id", command.Id).Add(args);
        return response;
    }
    public override string ToString()
    {
        string str = $"Id: {Id.ToString()}; Path: {Path}; Agrs<Type, Key, Value.ToString()>: ";
        foreach (var kvp in Args.ArgsDict)
        {
            string key = "null";
            string type = "null";
            string value = "null";
            if (kvp.Key != null) key = kvp.Key;
            if (kvp.Value != null)
            {
                type = kvp.Value.GetType().ToString();
                value = kvp.Value.ToSomeString();
            }
            str += $"<<{type}; {key}; {value}>>; ";
        }
        return str;
    }
}