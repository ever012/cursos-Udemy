using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Utilidades
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
        {
            //skip me permite asltarme un conjunto de registros y take me permite tomar una cantidad determinada de registros
            //PARA HACER UN SKIP Y TAKE ES OBLIGATORIO USAR UN ORDER BY CUANDO SE LALME ESTE EMTODO "PAGINAR"
            return queryable
                .Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecordsPorPagina).Take(paginacionDTO.RecordsPorPagina);
        }
    }
}
