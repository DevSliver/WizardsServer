using WizardsServer.GameLogic.MatchLogic;

namespace WizardsServer.GameLogic.StackLogic;

public class Stack
{
    readonly List<StackElement> _stack = new(16);
    public bool Empty => _stack.Count == 0;
    bool _resolving = false;
    internal Stack() { }
    internal void Add(StackElement effect)
    {
        _resolving = false;
        _stack.Add(effect);
    }
    public void RemoveTop() => _stack.RemoveAt(_stack.Count - 1);
    public void RemoveAt(int index) => _stack.RemoveAt(index);
    internal void Resolve(MatchContext context)
    {
        _resolving = true;
        for (int i = 0; i < _stack.Count; i++)
        {
            if (_resolving == false)
                return;
            StackElement effect = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            // effect.Resolve(context);
        }
    }
}
