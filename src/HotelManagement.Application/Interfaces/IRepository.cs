using System.Linq.Expressions;
using HotelManagement.Domain.Common;
namespace HotelManagement.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetAsync(Guid id);
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? filter = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    void Add(T entity);
}

public interface IUnitOfWork
{
    Task<T> LockedAsync<T>(Guid key, Func<Task<T>> operation);
}

public interface IHotelClock
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}
