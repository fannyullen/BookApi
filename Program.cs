using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BookApi.Models;
using BookApi.Data;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "db", "books.db");

Console.WriteLine("Använder databasfil: " + dbPath);
var connectionString = $"Data Source={dbPath}";

builder.Services.AddSingleton(new BookRepository(connectionString));
builder.Services.AddSingleton(new QuoteRepository(connectionString));

builder.Services.AddAuthorization();

/* builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("https://din-frontend.netlify.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
}); */

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => // Temporärt, but till AllowFrontend
        policy
            .AllowAnyOrigin() // Tillfälligt! Byt till Netflify-url som ovan med WithOrigins()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

string key = builder.Configuration["JwtKey"] ?? throw new Exception("JWT key missing");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

var app = builder.Build();

app.UseCors("AllowAll"); // Temporärt, but till AllowFrontend

app.UseAuthentication();
app.UseAuthorization();


app.UseHttpsRedirection();

app.MapGet("/api/books", (BookRepository repo) =>
{
    return Results.Ok(repo.GetAll());
});

app.MapGet("/api/books/{id}", (int id, BookRepository repo) =>
{
    var book = repo.GetById(id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
});

app.MapPost("/api/books", (Book book, BookRepository repo) =>
{
    var id = repo.Add(book);
    book.Id = id;
    return Results.Created($"/api/books/{id}", book);

});

app.MapPut("/api/books/{id}", (int id, Book book, BookRepository repo) =>
{
    if (repo.GetById(id) is null) return Results.NotFound();
    book.Id = id;
    return repo.Update(book) ? Results.Ok(book) : Results.Problem();
});

app.MapDelete("/api/books/{id}", (int id, BookRepository repo) =>
{
    return repo.Delete(id) ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/quotes", (QuoteRepository repo) =>
{
    return Results.Ok(repo.GetAll());
});

app.MapGet("/api/quotes/{id}", (int id, QuoteRepository repo) =>
{
    var quote = repo.GetById(id);
    return quote is not null ? Results.Ok(quote) : Results.NotFound();
});

app.MapPost("/api/quotes", (Quote quote, QuoteRepository repo) =>
{
    var newId = repo.Add(quote);
    quote.Id = newId;
    return Results.Created($"/api/quotes/{newId}", quote);
});

app.MapPut("/api/quotes/{id}", (int id, Quote updatedQuote, QuoteRepository repo) =>
{
    updatedQuote.Id = id;
    var success = repo.Update(updatedQuote);
    return success ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/api/quotes/{id}", (int id, QuoteRepository repo) =>
{
    var success = repo.Delete(id);
    return success ? Results.NoContent() : Results.NotFound();
});


var users = new List<(string Username, string Password)>
{
    ("user1", "password1"),
    ("user2", "password2")
};

app.MapPost("/api/login", (LoginRequest request) =>
{
    var user = users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
    if (user == default) return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenKey = Encoding.UTF8.GetBytes(key);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Username) }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = tokenString });
});

app.MapGet("/", () => "Book API is running!");

app.Run();

record LoginRequest(string Username, string Password);

