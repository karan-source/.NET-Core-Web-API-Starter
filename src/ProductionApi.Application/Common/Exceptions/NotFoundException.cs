namespace ProductionApi.Application.Common.Exceptions;

public sealed class NotFoundException(string entityName, object key)
    : Exception($"Entity \"{entityName}\" ({key}) was not found.")
{
    public string EntityName { get; } = entityName;

    public object Key { get; } = key;
}
