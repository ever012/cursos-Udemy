using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Servicios
{
    public interface IServicioHash
    {
        ResultadoHashDTO hash(string input);
        ResultadoHashDTO hash(string input, byte[] sal);
    }
}