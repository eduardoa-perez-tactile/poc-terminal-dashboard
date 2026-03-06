using Tva.Core;

namespace Tva.Application;

public sealed class LiveUpdateService
{
    private readonly IClock _clock;
    private readonly Random _random = new();

    public LiveUpdateService(IClock clock)
    {
        _clock = clock;
    }

    public void Initialize(ISessionState state)
    {
        state.Now = _clock.UtcNow.ToLocalTime();
        state.Alerts.Add(new AlertModel("Core", "Chrono relay synced", SeverityLevel.Info, state.Now));
        state.Alerts.Add(new AlertModel("Security", "Unauthorized branch scan", SeverityLevel.Warning, state.Now.AddMinutes(-3)));
        state.Alerts.Add(new AlertModel("Telemetry", "Packet drift above baseline", SeverityLevel.Critical, state.Now.AddMinutes(-7)));

        state.Events.Add("Boot sequence initialized.");
        state.Events.Add("Module registry loaded.");
        state.Events.Add("Navigation lattice online.");

        for (var i = 0; i < 24; i++)
        {
            state.WaveformSamples.Add(_random.Next(10, 90));
        }
    }

    public void Tick(ISessionState state)
    {
        state.TickCount++;
        state.Now = _clock.UtcNow.ToLocalTime();

        state.WaveformSamples.Add(_random.Next(5, 96));
        if (state.WaveformSamples.Count > 64)
        {
            state.WaveformSamples.RemoveAt(0);
        }

        if (state.TickCount % 12 == 0)
        {
            state.Events.Insert(0, $"[{state.Now:HH:mm:ss}] Event pulse #{state.TickCount / 12:000}");
        }

        if (state.Events.Count > 120)
        {
            state.Events.RemoveRange(120, state.Events.Count - 120);
        }

        if (state.TickCount % 50 == 0)
        {
            var severity = _random.Next(0, 3) switch
            {
                0 => SeverityLevel.Info,
                1 => SeverityLevel.Warning,
                _ => SeverityLevel.Critical
            };

            state.Alerts.Insert(
                0,
                new AlertModel(
                    "Watchtower",
                    $"Synthetic alert #{state.TickCount / 50:000}",
                    severity,
                    state.Now));
        }

        if (state.Alerts.Count > 60)
        {
            state.Alerts.RemoveRange(60, state.Alerts.Count - 60);
        }

        if (state.Notifications.Count > 12)
        {
            state.Notifications.RemoveRange(0, state.Notifications.Count - 12);
        }
    }
}
