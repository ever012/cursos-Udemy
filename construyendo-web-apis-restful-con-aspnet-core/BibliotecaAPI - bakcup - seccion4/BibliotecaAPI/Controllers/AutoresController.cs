using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController] //atributo que indica que esta clase es un controlador de API
    [Route("api/autores")] //ruta del controlador
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<AutoresController> logger;

        public AutoresController(ApplicationDbContext context, ILogger<AutoresController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        [HttpGet("/listado-de-autores")] // /listado-de-autores
        [HttpGet] // api/autores
        public async Task<IEnumerable<Autor>> Get()
        {
            /*return new List<Autor>()
            {
                new Autor{Id=1, Nombre="Autor 1"},
                new Autor{Id=2, Nombre="Autor 2"},
                new Autor{Id=3, Nombre="Autor 3"},
            };*/
            logger.LogTrace("Obteniendo listado de autores");
            //logger.LogDebug("Obteniendo listado de autores");
            //logger.LogInformation("Obteniendo listado de autores");
            //logger.LogWarning("Obteniendo listado de autores");
            //logger.LogError("Obteniendo listado de autores");
            //logger.LogCritical("Obteniendo listado de autores");
            return await context.Autores.ToListAsync();

        }

        /*[HttpGet("primero")] // api/autores/primero
        public async Task<Autor> getPrimerAutor()
        {
            return await context.Autores.FirstAsync();
        }*/

        /*[HttpGet("{parametro1}/{parametro2?}")] // api/autores/ever/castellon
        public IActionResult Get(string? parametro1, string parametro2 = "valor por defecto")
        {
            return Ok(new { parametro1, parametro2 });
        }*/

        [HttpGet("{id:int}")] // api/autores/1    (este se llama parametro de ruta)
        public async Task<ActionResult> Get(int id)
        {
            var autor = await context.Autores.Include(l => l.Libros).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (autor is null)
                return NotFound();

            return Ok(autor);
        }

        [HttpGet("{nombre:alpha}")]  //se usa alpha para letras en lugar de "string" sin numero o simbolos
        public async Task<IEnumerable<Autor>> Get(string nombre)
        {
            return await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
        }



        [HttpPost]
        public async Task<ActionResult> Post(Autor autor)
        {
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")] //api/autores/id
        public async Task<ActionResult> Put(int id, Autor autor)
        {
            if (id != autor.Id)
                return BadRequest("Los ids deben de coincidir");

            context.Update(autor);

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (registrosBorrados == 0)
                return NotFound();

            return Ok();

        }
    }
}
