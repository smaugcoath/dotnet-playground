using System;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;


namespace Pocs.Aspire.Infrastructure.Persistence;

internal class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Option<User>> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var result = await _context.Users.FindAsync(new object?[] { id }, cancellationToken);

        return result;
    }

    public Task<Unit> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);

        return Task.FromResult(Unit.Default);
    }

    public Task<Unit> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);

        return Task.FromResult(Unit.Default);
    }

    public async Task<bool> EmailExistsExceptForUser(Email email, UserId? userId = null, CancellationToken cancellationToken = default)
    {
        bool result = await _context.Users.AsNoTracking().AnyAsync(x => x.Email == email && x.Id != userId, cancellationToken: cancellationToken);

        return result;
    }
}
