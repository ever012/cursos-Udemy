using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
            // Preparación
            var nombreDB = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreDB);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<BibliotecaAPI.Controllers.V1.AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutoresV1 = null!;


            var controlador = new BibliotecaAPI.Controllers.V1.AutoresController(context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutoresV1); //envío nulos porque esas dependencias realmente no se usan, pero hay que pasarlas porque el constructor las requiere

            // Prueba
            var respuesta = await controlador.Get(1);

            // Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }


        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            // Preparación
            var nombreDB = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreDB);
            var mapper = ConfigurarAutoMapper();
            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<BibliotecaAPI.Controllers.V1.AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutoresV1 = null!;

            context.Autores.Add(new Autor { Nombres = "Ever", Apellidos = "Castellon" });
            context.Autores.Add(new Autor { Nombres = "Claudia", Apellidos = "Rodriguez" });
            await context.SaveChangesAsync();

            var context2 = ConstruirContext(nombreDB); //esto porque es recomendable no usar el mismo contexto con el que crea en este caso los autores que con el que se va a consultar en este caso esos autores

            var controlador = new BibliotecaAPI.Controllers.V1.AutoresController(context2, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutoresV1); //envío nulos porque esas dependencias realmente no se usan, pero hay que pasarlas porque el constructor las requiere

            // Prueba
            var respuesta = await controlador.Get(1);

            // Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }

    }
}
