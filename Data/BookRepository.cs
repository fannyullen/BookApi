using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using BookApi.Models;

namespace BookApi.Data;
public class BookRepository
{
    private readonly string _connectionString;

    public BookRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public IEnumerable<Book> GetAll()
    {

        using var connection = CreateConnection();
        return connection.Query<Book>("SELECT * FROM books");
    }

    public Book? GetById(int id)
    {
        using var connection = CreateConnection();
        return connection.QueryFirstOrDefault<Book>("SELECT * FROM books WHERE id = @id", new { id });
    }

    public int Add(Book book)
    {
        using var connection = CreateConnection();
        var sql = "INSERT INTO books (title, author, published) VALUES (@title, @author, @published); SELECT last_insert_rowid();";
        return connection.ExecuteScalar<int>(sql, new
        {
            title = book.Title,
            author = book.Author,
            published = book.Published
        });

    }
    public bool Update(Book book)
    {
        using var connection = CreateConnection();
        var sql = "UPDATE books SET title = @Title, author = @Author, published = @Published WHERE id = @Id";
        return connection.Execute(sql, book) > 0;
    }

    public bool Delete(int id)
    {
        using var connection = CreateConnection();
        return connection.Execute("DELETE FROM books WHERE id = @id", new { id }) > 0;
    }
}
