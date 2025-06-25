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
        Path = RemoveTrailingDot(path);
        Args = args;
    }
    public Command(string path)
    {
        Id = Guid.NewGuid();
        Path = RemoveTrailingDot(path);
        Args = new Args();
    }
    public static string RemoveTrailingDot(string path)
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
        return $"Id: {Id.ToString()}; Path: {Path}; Agrs<[Key], [Value.ToString()]>: {Args.ToString()}";
    }
}