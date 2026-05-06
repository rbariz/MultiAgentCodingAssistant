using CodingAssistant.Application.Abstractions;

namespace CodingAssistant.Infrastructure.Persistence.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly CodingAssistantDbContext _db;

        public UnitOfWork(CodingAssistantDbContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
    }
}
