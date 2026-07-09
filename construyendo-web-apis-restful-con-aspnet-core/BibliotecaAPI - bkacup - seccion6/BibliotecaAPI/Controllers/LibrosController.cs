using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
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
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<LibroDTO>> Get()
        {
            var libros = await context.Libros.Include(a => a.Autores).ToListAsync();
            var librosDTO = mapper.Map<IEnumerable<LibroDTO>>(libros);

            return librosDTO;
        }

        [HttpGet("{id:int}", Name ="ObtenerLibro")] // api/libros/1
        public async Task<ActionResult<LibroConAutoresDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(a => a.Autores)
                    .ThenInclude(al => al.Autor)
                .Where(x => x.Id == id).FirstOrDefaultAsync();
            
            if (libro is null)
                return NotFound();

            var libroDTO = mapper.Map<LibroConAutoresDTO>(libro);
            return libroDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "Debe existir al menos un autor");
                return ValidationProblem();
            }
                

            var AutoresIdsExisten = await context.Autores.Where(x => libroCreacionDTO!.AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if(AutoresIdsExisten.Count != libroCreacionDTO?.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO?.AutoresIds.Except(AutoresIdsExisten);
                var autoresNoExistenString = string.Join(",", autoresNoExisten!); //1,2,3
                var mensajeDeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeDeError);
                return ValidationProblem();
            }    

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarordenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        }
        private void AsignarordenAutores(Libro libro)
        {
            if (libro.Autores is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }
        }
           [HttpPut("{id:int}")]
           public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
           {
            if (libroCreacionDTO is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "Debe existir al menos un autor");
                return ValidationProblem();
            }


            var AutoresIdsExisten = await context.Autores.Where(x => libroCreacionDTO!.AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if (AutoresIdsExisten.Count != libroCreacionDTO?.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO?.AutoresIds.Except(AutoresIdsExisten);
                var autoresNoExistenString = string.Join(",", autoresNoExisten!); //1,2,3
                var mensajeDeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeDeError);
                return ValidationProblem();
            }

            var libroDB = await context.Libros.Include(a => a.Autores).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (libroDB is null)
                return NotFound();

               libroDB = mapper.Map(libroCreacionDTO, libroDB);
               AsignarordenAutores(libroDB);

               await context.SaveChangesAsync();
               return NoContent();

           }

           


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();
            
            if (registrosBorrados == 0)
                return NotFound();

            return NoContent();

        }

    }
}
