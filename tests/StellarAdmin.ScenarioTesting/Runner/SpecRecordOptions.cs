namespace StellarAdmin.ScenarioTesting.Runner;

// A record step's clip name and pacing; CursorSpeed scales how fast the cursor glides, so 2 is twice as fast
public sealed record SpecRecordOptions(
    string Name,
    int Line,
    bool Gif,
    int TypeDelay,
    float CursorSpeed,
    int Hold
);
