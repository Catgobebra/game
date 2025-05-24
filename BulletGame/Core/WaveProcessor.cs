using System.Collections.Generic;
using System;
using System.Linq;

public class WaveProcessor
{
    private readonly Stack<object> _waveStack;


    public WaveProcessor(Stack<object> waveStack)
    {
        _waveStack = waveStack;
    }

public void ProcessNextWave()
    {
        if (_waveStack.Count == 0) return;
        var wave = _waveStack.Pop();
        ProcessWaveItems(wave);
    }

    private void ProcessWaveItems(object waveItem)
    {
        var stack = new Stack<object>();
        stack.Push(waveItem);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current is Action action)
            {
                action.Invoke();
            }
            else if (current is IEnumerable<object> group)
            {
                foreach (var item in group.Reverse())
                {
                    stack.Push(item);
                }
            }
        }
    }
}