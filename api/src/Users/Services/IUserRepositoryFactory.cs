using System.Data.Common;

namespace Users.Services;

public interface IUserRepositoryFactory
{
    IUserRepository CreateFor(DbConnection connection, DbTransaction transaction);
}
