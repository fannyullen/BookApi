using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookApi.Models;
using BookApi.Data;

namespace BookApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly BookRepository _repo;

        public BooksController(BookRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_repo.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var book = _repo.GetById(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Book newBook)
        {
            var newId = _repo.Add(newBook);
            newBook.Id = newId;
            return CreatedAtAction(nameof(Get), new { id = newId }, newBook);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Book updatedBook)
        {
            updatedBook.Id = id;
            var success = _repo.Update(updatedBook);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _repo.Delete(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
