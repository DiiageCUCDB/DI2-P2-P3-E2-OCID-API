namespace OCID.Application.Interfaces;

public interface ILogService
{
    /// <summary>Creates a single log entry and returns its id.</summary>
    Task<Guid> CreateAsync(string eventName, Guid? userId = null, CancellationToken cancellationToken = default);
}