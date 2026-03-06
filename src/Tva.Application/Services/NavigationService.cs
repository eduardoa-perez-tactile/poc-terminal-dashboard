using Tva.Core;

namespace Tva.Application;

public sealed class NavigationService : INavigationService
{
    private readonly ISessionState _state;

    public NavigationService(ISessionState state)
    {
        _state = state;
    }

    public ScreenId ActiveScreenId => _state.ActiveScreenId;

    public void NavigateTo(ScreenId screenId)
    {
        if (_state.ActiveScreenId == screenId)
        {
            return;
        }

        _state.BackStack.Push(_state.ActiveScreenId);
        _state.ActiveScreenId = screenId;
    }

    public void NavigateNext(IReadOnlyList<NavigationEntry> entries)
    {
        if (entries.Count == 0)
        {
            return;
        }

        var currentIndex = entries
            .Select(static (entry, index) => new { entry, index })
            .FirstOrDefault(x => x.entry.ScreenId == _state.ActiveScreenId)?.index ?? -1;

        var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % entries.Count;
        NavigateTo(entries[nextIndex].ScreenId);
    }

    public void NavigatePrevious(IReadOnlyList<NavigationEntry> entries)
    {
        if (entries.Count == 0)
        {
            return;
        }

        var currentIndex = entries
            .Select(static (entry, index) => new { entry, index })
            .FirstOrDefault(x => x.entry.ScreenId == _state.ActiveScreenId)?.index ?? 0;

        var previousIndex = currentIndex - 1;
        if (previousIndex < 0)
        {
            previousIndex = entries.Count - 1;
        }

        NavigateTo(entries[previousIndex].ScreenId);
    }

    public void NavigateBack()
    {
        if (_state.BackStack.Count == 0)
        {
            return;
        }

        _state.ActiveScreenId = _state.BackStack.Pop();
    }
}
