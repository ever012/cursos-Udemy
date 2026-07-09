using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController] //atributo que indica que esta clase es un controlador de API
    [Route("api/v2/autores")] //ruta del controlador
    [Authorize]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<AutoresController> logger;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServicioAutores servicioAutoresV1;
        private const string contenedor = "autores";
        private const string cache = "autores-obtener";
        public AutoresController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos
            ,ILogger<AutoresController> logger,IOutputCacheStore outputCacheStore, IServicioAutores servicioAutoresV1)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
            this.servicioAutoresV1 = servicioAutoresV1;
        }

        [HttpGet] // api/autores
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            return await servicioAutoresV1.Get(paginacionDTO);


            /* //se comentarea porque ahora existe el servicio servicioAutoresV1
             * var queryable = context.Autores.AsQueryable(); //AsQueryable permite armar el query parte por parte
            await HttpContext.InsertarParametrosPaginacionCabecera(queryable);
            var autores = await queryable.OrderBy(x => x.Nombres).Paginar(paginacionDTO).ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;*/

        }


        [HttpGet("{id:int}", Name = "ObtenerAutorV2")] // api/autores/1    (este se llama parametro de ruta)
        [AllowAnonymous]
        [EndpointSummary("Obtiene un autor por su id")]
        [EndpointDescription("Este endpoint permite obtener un autor por su id, incluyendo sus libros")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[OutputCache(Tags = [cache])]
        public async Task<ActionResult<AutorConLibrosDTO>> Get([Description("El Id del Autor")] int id, bool incluirLibros = false)
        {
            var queryable = context.Autores.AsQueryable();

            if (incluirLibros)
            {
                queryable = queryable.Include(l => l.Libros)
                    .ThenInclude(lb => lb.Libro);
            }
            var autor = await queryable.Where(x => x.Id == id).FirstOrDefaultAsync();


            /*var autor = await context.Autores
                .Include(l => l.Libros)
                    .ThenInclude(lb => lb.Libro)
                .Where(x => x.Id == id).FirstOrDefaultAsync();*/
            if (autor is null)
                return NotFound();

            var autorDTO = mapper.Map<AutorConLibrosDTO>(autor);

            return autorDTO;
        }







        [HttpGet("{nombre:alpha}")]  //se usa alpha para letras en lugar de "string" sin numero o simbolos
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<Autor>> Get(string nombre)
        {
            return await context.Autores.Where(x => x.Nombres.Contains(nombre)).ToListAsync();
        }

        [HttpGet("filtrar")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {
            var queryable = context.Autores.AsQueryable();   //a esto se llama ejecucion diferida

            //filtrado dinamico
            if (!string.IsNullOrEmpty(autorFiltroDTO.Nombres))
                queryable = queryable.Where(x => x.Nombres.Contains(autorFiltroDTO.Nombres));

            if (!string.IsNullOrEmpty(autorFiltroDTO.Apellidos))
                queryable = queryable.Where(x => x.Apellidos.Contains(autorFiltroDTO.Apellidos));

            if (autorFiltroDTO.IncluirLibros)
                queryable = queryable.Include(x => x.Libros).ThenInclude(x => x.Libro);

            if (autorFiltroDTO.TieneFoto.HasValue)
            {
                if (autorFiltroDTO.TieneFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);
                }
            } 

            if(autorFiltroDTO.TieneLibros.HasValue) //valida si hay un valor
            {
                if(autorFiltroDTO.TieneLibros.Value) //validar si es verdadero
                {
                    queryable = queryable.Where(x => x.Libros.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Libros.Any());
                }

            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x => x.Libros.Any(y => y.Libro!.Titulo.Contains(autorFiltroDTO.TituloLibro)));
            }

            if(!string.IsNullOrEmpty(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdenAascendente ? "ascending" : "desceending";

                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");
                }
                catch(Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Nombres);
                    logger.LogError(ex.Message, ex);
                }

            }
            else
            {
                queryable = queryable.OrderBy(x => x.Nombres);
            }



            var autores = await queryable.Paginar(autorFiltroDTO.PaginacionDTO).ToListAsync();


            if (autorFiltroDTO.IncluirLibros)
            {
                var autoresDTO = mapper.Map<IEnumerable<AutorConLibrosDTO>>(autores);
                return Ok(autoresDTO);
            }
            else
            {
                var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoresDTO);
            }

            
        }

        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache,default); //limpiar cache cuando se cree un nuevo autor

            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV2", new { id = autor.Id }, autorDTO);
        }

        [HttpPost("contenedor-foto")]
        public async Task<ActionResult> Post([FromForm] AutorCreacionDTOConFoto autorCreacionDTO)
        {
            
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            
            if(autorCreacionDTO.Foto is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, autorCreacionDTO.Foto);
                autor.Foto = url;
            }
            
            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default); //limpiar cache cuando se cree un nuevo autor

            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV2", new { id = autor.Id }, autorDTO);
        }


        [HttpPut("{id:int}")] //api/autores/id
        public async Task<ActionResult> Put(int id,[FromForm] AutorCreacionDTOConFoto autorCreacionDTO)
        {
            var existeAutor = await context.Autores.AnyAsync(a => a.Id == id);
            if (!existeAutor)
                return NotFound();

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            if(autorCreacionDTO.Foto is not null)
            {
                var fotoActual = await context.Autores.Where(x => x.Id == id).Select(x => x.Foto).FirstAsync();
                var url = await almacenadorArchivos.Editar(fotoActual, contenedor, autorCreacionDTO.Foto);
                autor.Foto = url;
            }



            context.Update(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default); //limpiar cache cuando se cree un nuevo autor

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
            await outputCacheStore.EvictByTagAsync(cache, default); //limpiar cache cuando se cree un nuevo autor

            return NoContent(); //204


        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if (autor is null)
                return NotFound();

            context.Remove(autor);
            await context.SaveChangesAsync();
            await almacenadorArchivos.Borrar(autor.Foto, contenedor);
            await outputCacheStore.EvictByTagAsync(cache, default); //limpiar cache cuando se cree un nuevo autor

            /*var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (registrosBorrados == 0)
                return NotFound();*/

            return NoContent();  //204

        }
    }
}
