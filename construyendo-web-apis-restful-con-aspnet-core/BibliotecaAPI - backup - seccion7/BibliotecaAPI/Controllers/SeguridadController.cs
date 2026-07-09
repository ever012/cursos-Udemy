using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/seguridad")]
    public class SeguridadController : ControllerBase
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorLimitadoPorTiempo;
        private readonly IServicioHash servicioHash;

        public SeguridadController(IDataProtectionProvider protectionProvider, IServicioHash servicioHash)
        {
            protector = protectionProvider.CreateProtector("SeguridadController"); //eñ string de proposito es la lave usada para encriptar y desencriptar, NO ES LA LLAVE, ES PARTE DE LA LLAVE
            protectorLimitadoPorTiempo = protector.ToTimeLimitedDataProtector();
            this.servicioHash = servicioHash;
        }

        [HttpGet("encriptar-limitado-por-tiempo")]
        public ActionResult EncriptarLimitadoPorTiempo(string textoPlano)
        {
            var textoEncriptado = protectorLimitadoPorTiempo.Protect(textoPlano,lifetime: TimeSpan.FromSeconds(30));
            return Ok(textoEncriptado);
        }

        [HttpGet("hash")]
        public ActionResult Hash(string textoPlano)
        {
            var hash1 = servicioHash.hash(textoPlano);
            var hash2 = servicioHash.hash(textoPlano);
            var has3 = servicioHash.hash(textoPlano, hash2.Sal);
            var resultado = new { Hash1 = hash1, Hash2 = hash2, Hash3 = has3 };
            return Ok(resultado);
        }



        [HttpGet("desencriptar-limitado-por-tiempo")]
        public ActionResult DesencriptarLimitadoPorTiempo(string textoCifrado)
        {
            var textoPlano = protectorLimitadoPorTiempo.Unprotect(textoCifrado);
            return Ok(textoPlano);


        }


        [HttpGet("encriptar")]
        public ActionResult Encriptar(string textoPlano)
        {
            var textoEncriptado = protector.Protect(textoPlano);
            return Ok(textoEncriptado);
        }

        [HttpGet("desencriptar")]
        public ActionResult Desencriptar(string textoCifrado)
        {
                var textoPlano = protector.Unprotect(textoCifrado);
                return Ok(textoPlano);
            

        }

    }
}
