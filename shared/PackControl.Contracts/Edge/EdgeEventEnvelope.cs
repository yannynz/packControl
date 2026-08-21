namespace PackControl.Contracts.Edge;

public sealed record EdgeEventEnvelope(
    Guid EventId,
    string EventType,
    DateTime OccurredAtUtc,
    string SourceMachine,
    string SourcePath,
    string EntityRef,
    string Hash,
    Dictionary<string, string> Payload);
