using OCID.Infrastructure.Entities;

namespace OCID.Infrastructure.Interfaces;

public interface ILogRepository
{
    Task<Logs> AddAsync(Logs log, CancellationToken cancellationToken = default);
}