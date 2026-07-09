using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroValidacionLibro : IAsyncActionFilter
    {
        private readonly ApplicationDbContext dbContext;

        public FiltroValidacionLibro(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(!context.ActionArguments.TryGetValue("libroCreacionDTO", out var value) || 
                value is not LibroCreacionDTO libroCreacionDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es válido");
                context.Result = context.ModelState.ContruirProblemDetail(); //con solo colocar un context.Result y asingnar un valor, se está haciendo un cortocircuitlo a la tuberia de filtros de tal manera que vamos a evitar que se ejecute la accion o enpoint y los demas filtros
                return;
            }


            if (libroCreacionDTO is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "Debe existir al menos un autor");
                context.Result = context.ModelState.ContruirProblemDetail(); //con solo colocar un context.Result y asingnar un valor, se está haciendo un cortocircuitlo a la tuberia de filtros de tal manera que vamos a evitar que se ejecute la accion o enpoint y los demas filtros
                return;
            }


            var AutoresIdsExisten = await dbContext.Autores.Where(x => libroCreacionDTO!.AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if (AutoresIdsExisten.Count != libroCreacionDTO?.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO?.AutoresIds.Except(AutoresIdsExisten);
                var autoresNoExistenString = string.Join(",", autoresNoExisten!); //1,2,3
                var mensajeDeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeDeError);
                context.Result = context.ModelState.ContruirProblemDetail(); 
                return;
            }


            await next();
        }
    }
}
