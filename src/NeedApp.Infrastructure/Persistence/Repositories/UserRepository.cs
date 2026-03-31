using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        => !await DbSet.AnyAsync(u => u.Email == email, cancellationToken);
}
