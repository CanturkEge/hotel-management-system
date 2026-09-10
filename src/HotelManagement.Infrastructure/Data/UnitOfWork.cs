using HotelManagement.Application.Common;
using HotelManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace HotelManagement.Infrastructure.Data;

public sealed class UnitOfWork(HotelDbContext db) : IUnitOfWork
{
    public async Task<T> LockedAsync<T>(Guid key,Func<Task<T>> operation)
    {
        await using var transaction=await db.Database.BeginTransactionAsync();
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key.ToString()}, 0))");
            db.ChangeTracker.Clear();
            var result=await operation();
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch(DbUpdateException ex) when(ex.InnerException is PostgresException {SqlState:"23505"})
        {
            throw new AppException("Aynı kayıt zaten var. Sayfayı yenileyip tekrar kontrol edin.");
        }
    }
}
