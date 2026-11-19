using OCID.Application.Interfaces;
using OCID.Infrastructure.Entities;
using OCID.Infrastructure.Interfaces;

namespace OCID.Application.Services;

public class LogService : ILogService
{
    private readonly ILogRepository _logRepository;

    public LogService(ILogRepository logRepository) => _logRepository = logRepository;

    public async Task<Guid> CreateAsync(string eventName, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name is required.", nameof(eventName));

        var log = new Logs
        {
            Event = eventName.Trim(),
            UserId = userId
        };

        var created = await _logRepository.AddAsync(log, cancellationToken);
        return created.Id;
    }
}