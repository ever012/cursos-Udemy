using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Utilidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;


namespace BibliotecaAPITests.Utilidades
{
    public class BasePruebas  //clase base para todas o la mayoria de las pruebas (para poder probar emtodos de un controlador)
    {
        protected ApplicationDbContext ConstruirContext(string nombreDB)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDB).Options;

            var dbContext = new ApplicationDbContext(opciones);
            return dbContext;
        }

        //configurar Automapper
        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(opciones =>
            {
                opciones.AddProfile(new AutoMapperProfiles());
            }, NullLoggerFactory.Instance);
            return config.CreateMapper();
        }
    }
}
