using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class FlipCardCommand : ICommand
{
    private Card _card;
    
    public FlipCardCommand(Card card)
    {
        _card = card;
    }
    
    public void Execute()
    {
        _card.FlipUp();
    }
    
    public void Undo()
    {
        _card.FlipDown();
    }
}

public class CommandManager : MonoBehaviour
{
    private Stack<ICommand> _commandHistory = new Stack<ICommand>();
    private int _maxHistorySize = 50;
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _commandHistory.Push(command);
        
        if (_commandHistory.Count > _maxHistorySize)
        {
            var oldCommands = _commandHistory.ToArray();
            _commandHistory.Clear();
            for (int i = oldCommands.Length - _maxHistorySize; i < oldCommands.Length; i++)
            {
                _commandHistory.Push(oldCommands[i]);
            }
        }
    }
    
    public void UndoLastCommand()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
        }
    }
}
