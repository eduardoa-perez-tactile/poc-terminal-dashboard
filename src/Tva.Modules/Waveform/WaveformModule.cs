using Tva.Application;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class WaveformModule : IAppModule
{
    public AppModuleInfo Metadata { get; } = new(
        "waveform",
        "Waveform",
        "Simple live timeline widget.",
        "[W]");

    public IReadOnlyList<NavigationEntry> NavigationEntries { get; } =
    [
        new("waveform", ScreenCatalog.Waveform, "Waveform", "5", 50)
    ];

    public IReadOnlyList<IScreenProvider> Screens { get; } =
    [
        new WaveformScreenProvider()
    ];

    public IReadOnlyList<PanelModel> GetDashboardPanels(AppSessionState state)
    {
        var latest = state.WaveformSamples.Count > 0
            ? state.WaveformSamples[^1]
            : 0;

        return
        [
            new PanelModel(
                "Timeline",
                [
                    $"Latest amplitude: {latest}",
                    $"Samples tracked: {state.WaveformSamples.Count}"
                ])
        ];
    }

    public IReadOnlyList<StatusItem> GetStatusItems(AppSessionState state)
    {
        var latest = state.WaveformSamples.Count > 0
            ? state.WaveformSamples[^1]
            : 0;

        return
        [
            new StatusItem("Pulse", latest.ToString())
        ];
    }

    private sealed class WaveformScreenProvider : IScreenProvider
    {
        public ScreenId ScreenId => ScreenCatalog.Waveform;

        public string ModuleId => "waveform";

        public ScreenViewModel Build(AppSessionState state)
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
