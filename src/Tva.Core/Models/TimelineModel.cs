namespace Tva.Core;

public sealed record TimelineModel(
    string Title,
    IReadOnlyList<int> Samples,
    int Min = 0,
    int Max = 100);
