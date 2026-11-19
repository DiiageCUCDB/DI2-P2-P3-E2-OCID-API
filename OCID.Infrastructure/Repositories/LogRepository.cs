using OCID.Infrastructure.Data;
using OCID.Infrastructure.Entities;
using OCID.Infrastructure.Interfaces;

namespace OCID.Infrastructure.Repositories;

public class LogRepository : ILogRepository
{
    private readonly AppDbContext _db;

    public LogRepository(AppDbContext db) => _db = db;

    public async Task<Logs> AddAsync(Logs log, CancellationToken cancellationToken = default)
    {
        _db.Logs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        return log;
    }
}