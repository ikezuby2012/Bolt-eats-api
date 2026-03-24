using Application.Abstractions.Data;
using Application.Abstractions.Interface;
using Infrastructure.Database;
using Infrastructure.UnitOfWork.Repository;

namespace Infrastructure.UnitOfWork;
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public IUserRepository UserRepository { get; init; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;

        UserRepository = new UserRepository(db);
    }

    public void Save()
    {
        _db.SaveChanges();
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
