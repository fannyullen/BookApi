using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using BookApi.Models;

namespace BookApi.Data;

public class QuoteRepository
{
    private readonly string _connectionString;

    public QuoteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public IEnumerable<Quote> GetAll()
    {
        using var connection = CreateConnection();
        return connection.Query<Quote>("SELECT * FROM quotes");
    }

    public Quote? GetById(int id)
    {
        using var connection = CreateConnection();
        return connection.QueryFirstOrDefault<Quote>("SELECT * FROM quotes WHERE id = @id", new { id });
    }

    public int Add(Quote quote)
    {
        using var connection = CreateConnection();
        var sql = "INSERT INTO quotes (text, author) VALUES (@Text, @Author); SELECT last_insert_rowid();";
        return connection.ExecuteScalar<int>(sql, quote);
    }

    public bool Update(Quote quote)
    {
        using var connection = CreateConnection();
        var sql = "UPDATE quotes SET text = @Text, Author = @Author WHERE id = @Id";
        return connection.Execute(sql, quote) > 0;
    }

    public bool Delete(int id)
    {
        using var connection = CreateConnection();
        return connection.Execute("DELETE FROM quotes WHERE id = @id", new { id }) > 0;
    }
}
