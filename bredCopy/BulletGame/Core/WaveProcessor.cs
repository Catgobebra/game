using System;
using System.Collections.Generic;

public interface IWaveItem
{
    void Process();
}

public class ActionWaveItem : IWaveItem
{
    private readonly Action _action;

    public ActionWaveItem(Action action)
    {
        _action = action;
    }

    public void Process()
    {
        _action?.Invoke();
    }
}

public class GroupWaveItem : IWaveItem
{
    private readonly IEnumerable<IWaveItem> _items;

    public GroupWaveItem(IEnumerable<IWaveItem> items)
    {
        _items = items;
    }

    public void Process()
    {
        var stack = new Stack<IWaveItem>();
        foreach (var item in _items)
            stack.Push(item);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            current.Process();
        }
    }
}

public class WaveProcessor
{
    private readonly Stack<IWaveItem> _waveStack;

    public WaveProcessor(Stack<IWaveItem> waveStack)
    {
        _waveStack = waveStack;
    }

    public void ProcessNextWave()
    {
        if (_waveStack.Count == 0) return;

        var wave = _waveStack.Pop();
        wave.Process();
    }

    public void ProcessWaveItems(IWaveItem item)
    {
        _waveStack.Push(item);
        ProcessNextWave();
    }
}