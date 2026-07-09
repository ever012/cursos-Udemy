using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController] //atributo que indica que esta clase es un controlador de API
    [Route("api/autores")] //ruta del controlador
    [Authorize]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet] // api/autores
        [AllowAnonymous]
        public async Task<IEnumerable<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;

        }


        [HttpGet("{id:int}", Name ="ObtenerAutor")] // api/autores/1    (este se llama parametro de ruta)
        public async Task<ActionResult<AutorConLibrosDTO>> Get(int id)
        {
            var autor = await context.Autores
                .Include(l => l.Libros)
                    .ThenInclude(lb => lb.Libro)
                .Where(x => x.Id == id).FirstOrDefaultAsync();
            if (autor is null)
                return NotFound();

            var autorDTO = mapper.Map<AutorConLibrosDTO>(autor);

            return autorDTO;
        }

        [HttpGet("{nombre:alpha}")]  //se usa alpha para letras en lugar de "string" sin numero o simbolos
        public async Task<IEnumerable<Autor>> Get(string nombre)
        {
            return await context.Autores.Where(x => x.Nombres.Contains(nombre)).ToListAsync();
        }



        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")] //api/autores/id
        public async Task<ActionResult> Put(int id, AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;
            context.Update(autor);
            await context.SaveChangesAsync();

            return NoContent(); //204
        }

        [HttpPatch("{id:int}")] //api/autores/id
        public async Task<ActionResult> Patch(int id,JsonPatchDocument<AutorPatchDTO> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest();

            var autorDB = await context.Autores.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (autorDB is null)
                return NotFound();

            var autorPatchDTO = mapper.Map<AutorPatchDTO>(autorDB);

            patchDoc.ApplyTo(autorPatchDTO, ModelState);

            var esValido = TryValidateModel(autorPatchDTO);

            if (!esValido)
                return ValidationProblem();


            mapper.Map(autorPatchDTO, autorDB);
            await context.SaveChangesAsync();

            return NoContent(); //204


        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (registrosBorrados == 0)
                return NotFound();

            return NoContent();  //204

        }
    }
}
