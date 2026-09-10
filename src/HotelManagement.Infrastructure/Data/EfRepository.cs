using System.Linq.Expressions;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;
namespace HotelManagement.Infrastructure.Data;

public sealed class EfRepository<T>(HotelDbContext db) : IRepository<T> where T:BaseEntity
{
    public Task<T?> GetAsync(Guid id) => db.Set<T>().FirstOrDefaultAsync(x=>x.Id==id);
    public Task<List<T>> ListAsync(Expression<Func<T,bool>>? filter=null)
    {
        IQueryable<T> query=db.Set<T>().AsNoTracking();
        if(filter!=null) query=query.Where(filter);
        return query.ToListAsync();
    }
    public Task<bool> AnyAsync(Expression<Func<T,bool>> filter) => db.Set<T>().AnyAsync(filter);
    public void Add(T entity) => db.Set<T>().Add(entity);
}
