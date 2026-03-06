using Tva.Core;

namespace Tva.Application;

public interface INavigationService
{
    ScreenId ActiveScreenId { get; }

    void NavigateTo(ScreenId screenId);
    void NavigateNext(IReadOnlyList<NavigationEntry> entries);
    void NavigatePrevious(IReadOnlyList<NavigationEntry> entries);
    void NavigateBack();
}
