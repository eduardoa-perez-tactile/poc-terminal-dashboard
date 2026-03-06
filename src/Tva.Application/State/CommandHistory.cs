namespace Tva.Application;

public sealed class CommandHistory
{
    private readonly List<string> _entries = [];
    private int _cursor;
    private string _inProgress = string.Empty;

    public IReadOnlyList<string> Entries => _entries;

    public void Add(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        _entries.Add(command.Trim());
        _cursor = _entries.Count;
        _inProgress = string.Empty;
    }

    public string Previous(string currentInput)
    {
        if (_entries.Count == 0)
        {
            return currentInput;
        }

        if (_cursor == _entries.Count)
        {
            _inProgress = currentInput;
        }

        _cursor = Math.Max(0, _cursor - 1);
        return _entries[_cursor];
    }

    public string Next()
    {
        if (_entries.Count == 0)
        {
            return string.Empty;
        }

        if (_cursor < _entries.Count - 1)
        {
            _cursor++;
            return _entries[_cursor];
        }

        _cursor = _entries.Count;
        return _inProgress;
    }
}
