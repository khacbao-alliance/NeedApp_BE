using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;
using NeedApp.Infrastructure.Persistence;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
