using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public LibrosController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Libro>> Get()
        {
            return await context.Libros.ToListAsync();
        }

        [HttpGet("{id:int}")] // api/libros/1
        public async Task<ActionResult> Get(int id)
        {
            var libro = await context.Libros.Include(a => a.Autor).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (libro is null)
                return NotFound();

            return Ok(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(Libro libro)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId); //AnyAsync retorna true o false

            if (!existeAutor)
            {
                ModelState.AddModelError(nameof(libro.AutorId), $"El Autor de id {libro.AutorId} o existe");
                return ValidationProblem();
            }
                
            //return BadRequest($"El Autor de id {libro.AutorId} o existe");

            context.Add(libro);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Libro libro)
        {
            if (id != libro.Id)
                return BadRequest("Los ids deben de coincidir");

            var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId); //AnyAsync retorna true o false
            if (!existeAutor)
                return BadRequest($"El Autor de id {libro.AutorId} o existe");

            context.Update(libro);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();
            
            if (registrosBorrados == 0)
                return NotFound();

            return Ok();

        }

    }
}
