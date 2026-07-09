using AutoMapper;
using Azure;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/libros/{libroId}/comentarios")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "comentarios-obtener";

        public ComentariosController(ApplicationDbContext context, IMapper mapper, IServicioUsuarios servicioUsuarios
            ,IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioUsuarios = servicioUsuarios;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound();

            var comentarios = await context.Comentarios.Include(x => x.Usuario)
                .Where(x => x.LibroId == libroId).OrderByDescending(x => x.FechaPublicacion).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);




        }

        [HttpGet("{id}", Name = "ObtenerComentarioV2")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<ComentarioDTO>> Get(Guid id)
        {
            var comentario = await context.Comentarios.Include(x => x.Usuario).FirstOrDefaultAsync(x => x.Id == id);
            if (comentario == null)
                return NotFound();

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound();

            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.UsuarioId = usuario.Id;
            comentario.FechaPublicacion = DateTime.UtcNow;

            context.Add(comentario);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("ObtenerComentarioV2", new { id = comentario.Id, libroId }, comentarioDTO);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int libroId, JsonPatchDocument<ComentarioPatchDTO> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest();

            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound();

            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
                return NotFound();

            var comentarioDB = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentarioDB is null)
                return NotFound();

            if(comentarioDB.UsuarioId != usuario.Id)  //validacion para editar mi comentario y no el de otro
                return Forbid(); //403 Forbidden



            var comentarioPatchDTO = mapper.Map<ComentarioPatchDTO>(comentarioDB);

            patchDoc.ApplyTo(comentarioPatchDTO, ModelState);

            var esValido = TryValidateModel(comentarioPatchDTO);
            if (!esValido)
                return ValidationProblem();

            mapper.Map(comentarioPatchDTO, comentarioDB);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound();

            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
                return NotFound();

            var comentarioDb = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if(comentarioDb is null)
                return NotFound();

            if(comentarioDb.UsuarioId != usuario.Id)
                return Forbid();

            comentarioDb.EstaBorrado = true;
            context.Update(comentarioDb);
            //context.Remove(comentarioDb);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            /*var comentario = await context.Comentarios.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (comentario == 0)
                return NotFound();*/

            return NoContent();
        }
    }
}
