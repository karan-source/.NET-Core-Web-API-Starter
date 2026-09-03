namespace ProductionApi.Domain.Common;

public abstract class BaseAuditableEntity
{
    // Version 7 GUIDs are time-ordered, which keeps clustered index inserts sequential.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }
}
