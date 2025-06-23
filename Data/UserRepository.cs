using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using BookApi.Models;

namespace BookApi.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public User? GetByUsernameAndPassword(string username, string password)
    {
        using var connection = CreateConnection();
        return connection.QueryFirstOrDefault<User>(
            "SELECT * FROM users WHERE username = @username AND password = @password",
            new { username, password });
    }

    public int Add(User user)
    {
        using var connection = CreateConnection();
        var sql = "INSERT INTO users (username, password) VALUES (@username, @password); SELECT last_insert_rowid();";
        return connection.ExecuteScalar<int>(sql, new { username = user.Username, password = user.Password });
    }

    public User? GetByUsername(string username)
    {
        using var connection = CreateConnection();
        return connection.QueryFirstOrDefault<User>("SELECT * FROM users WHERE username = @username", new { username });
    }

    public IEnumerable<User> GetAll()
    {
        using var connection = CreateConnection();
        return connection.Query<User>("SELECT * FROM users");
    }

    public User? GetById(int id)
    {
        using var connection = CreateConnection();
        return connection.QueryFirstOrDefault<User>("SELECT * FROM users WHERE id = @id", new { id });
    }
}
