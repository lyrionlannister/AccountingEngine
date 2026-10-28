using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Users.Persistence;
using Users.Services;

namespace Users.Repositories;

internal sealed class UserRepositoryFactory : IUserRepositoryFactory
{
    public IUserRepository CreateFor(DbConnection connection, DbTransaction transaction)
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(connection).Options;
        var context = new UsersDbContext(options);
        context.Database.UseTransaction(transaction);

        return new UserRepository(context);
    }
}
