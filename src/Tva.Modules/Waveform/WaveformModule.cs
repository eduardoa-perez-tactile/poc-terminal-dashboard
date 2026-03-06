using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class WaveformModule : IModule
{
    public string Id => "waveform";

    public string DisplayName => "Waveform";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Waveform, "Waveform", "5", 50));
        context.RegisterScreen(ScreenCatalog.Waveform, new WaveformScreenProvider());

        context.RegisterDashboardPanel(state =>
        {
            var latest = state.WaveformSamples.Count > 0
                ? state.WaveformSamples[^1]
                : 0;

            return new PanelModel(
                "Timeline",
                [
                    $"Latest amplitude: {latest}",
                    $"Samples tracked: {state.WaveformSamples.Count}"
                ]);
        });

        context.RegisterStatusItem(state =>
        {
            var latest = state.WaveformSamples.Count > 0
                ? state.WaveformSamples[^1]
                : 0;

            return new StatusItem("Pulse", latest.ToString());
        });
    }

    private sealed class WaveformScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            return new ScreenViewModel(
                ScreenCatalog.Waveform,
                "Waveform / Timeline",
                "Mock live widget for periodic updates",
                [],
                null,
                [],
                [],
                new TimelineModel("Temporal Pulse", state.WaveformSamples.ToList()),
                null,
                "Rendered with text bars in TUI; reusable for desktop renderer.");
        }
    }
}
