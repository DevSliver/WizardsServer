namespace WizardsServer.ServerLogic;

public class CommandQueue
{
    private readonly Dictionary<long, string[]> _pendingCommands = new();
    private long _expectedNumber = 1;

    public void Enqueue(long number, string[] args)
    {
        _pendingCommands[number] = args;
    }
    public bool TryDequeue(out string[] args)
    {
        args = Array.Empty<string>();
        if (!_pendingCommands.TryGetValue(_expectedNumber, out var getedArgs))
            return false;

        _pendingCommands.Remove(_expectedNumber);
        args = getedArgs;
        _expectedNumber++;
        return true;
    }
}