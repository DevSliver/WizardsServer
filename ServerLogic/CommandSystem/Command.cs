using MessagePack;

namespace WizardsServer.ServerLogic.CommandSystem;

[MessagePackObject(AllowPrivate = true)]
public partial class Command
{
    [IgnoreMember]
    public readonly Guid Id;
    [Key(0)]
    public readonly string Path;
    [Key(1)]
    public readonly Args Args;

    [SerializationConstructor]
    public Command(string path, Args args)
    {
        Id = Guid.NewGuid();
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
        response.Args.Add("Message", message);
        response.Args.Add("Id", command.Id);
        response.Args.Add("Path", command.Path);
        return response;
    }
    public static Command Response(Command command, Args args)
    {
        Command response = new Command("Response.Command");
        response.Args.Add("Id", command.Id).Add("Path", command.Path).Add(args);
        return response;
    }
}