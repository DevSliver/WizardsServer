namespace WizardsServer.ServerLogic.CommandSystem;

using MessagePack;

[Union(0, typeof(Arg<int>))]
[Union(1, typeof(Arg<float>))]
[Union(2, typeof(Arg<string>))]
[Union(3, typeof(Arg<bool>))]
[Union(4, typeof(Arg<Guid>))]

[Union(5, typeof(Arg<List<int>>))]
[Union(6, typeof(Arg<List<float>>))]
[Union(7, typeof(Arg<List<string>>))]
[Union(8, typeof(Arg<List<bool>>))]
[Union(9, typeof(Arg<List<Guid>>))]
public interface IArg
{
    string ToString();
}
[MessagePackObject]
public struct Arg<T> : IArg
{
    [Key(0)]
    public readonly T Value;
    public Arg(T value)
    {
        Value = value;
    }
    public new string ToString() => Value.ToString();
}
[MessagePackObject(AllowPrivate = true)]
public partial class Args
{
    [Key(0)]
    private readonly Dictionary<string, IArg> _args = new();
    [IgnoreMember]
    public IReadOnlyDictionary<string, IArg> ArgsDict => _args;

    [SerializationConstructor]
    public Args(Dictionary<string, IArg> args)
    {
        _args = args;
    }
    public Args() { }

    public bool TryGet<T>(string key, out T? value)
    {
        value = default;
        if (!(_args[key] is Arg<T> arg))
            return false;
        value = arg.Value;
        return true;
    }
    public bool TryGet(string key, out string value)
    {
        value = "";
        if (!(_args[key] is Arg<string> arg))
            return false;
        value = arg.Value;
        return true;
    }
    public bool TryAdd<T>(string key, T value)
    {
        if (_args.ContainsKey(key))
            return false;
        _args.Add(key, new Arg<T>(value));
        return true;
    }
    public Args Add<T>(string key, T value)
    {
        _args[key] = new Arg<T>(value);
        return this;
    }
    public Args Add(Args newArgs)
    {
        var newArgsDict = newArgs.ArgsDict;
        // don't use Add<T> here! Because Add<T> creates new Arg.
        foreach(var kvp in newArgsDict)
            _args[kvp.Key] = kvp.Value;
        return this;
    }
    public Args AddResponse(Command responseTo, Args responseArgs) =>
        Add("Id", responseTo.Id).Add("Path", responseTo.Path).Add(responseArgs);
    public override string ToString()
    {
        string str = "";
        foreach (var kvp in _args)
        {
            string key = "[null]";
            string value = "[null]";
            if (kvp.Key != null) key = kvp.Key;
            if (kvp.Value != null) value = kvp.Value.ToSomeString();
            str += $"<<{key}; {value}>>; ";
        }
        return str;
    }
}