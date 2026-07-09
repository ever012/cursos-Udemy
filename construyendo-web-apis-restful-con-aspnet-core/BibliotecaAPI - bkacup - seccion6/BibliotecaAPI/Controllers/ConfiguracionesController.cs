using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/configuraciones")]
    public class ConfiguracionesController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly PagosProcesamiento pagosProcesamiento;
        private readonly PersonaOpciones _opcionesPersona;
        private readonly IConfigurationSection seccion_01;
        private readonly IConfigurationSection seccion_02;

        public ConfiguracionesController(IConfiguration configuration, IOptionsSnapshot<PersonaOpciones> opcionesPersona, PagosProcesamiento pagosProcesamiento)
        {
            this.configuration = configuration;
            this.pagosProcesamiento = pagosProcesamiento;
            _opcionesPersona = opcionesPersona.Value;
            seccion_01 = configuration.GetSection("seccion_1");
            seccion_02 = configuration.GetSection("seccion_2");
        }

        [HttpGet("options-monitor")]
        public ActionResult GetTarifas()
        {
            return Ok(pagosProcesamiento.ObtenerTarifas());
        }

        [HttpGet("seccion_1_opciones")]
        public ActionResult GetSeccionOpciones()
        {
            return Ok(_opcionesPersona);
        }


        [HttpGet("proveedores")]
        public IActionResult GetProveedores()
        {
            var valor = configuration.GetValue<string>("quien_soy");

            return Ok(new { valor });
        }

        [HttpGet("obtenertodos")]
        public ActionResult<string> getObtenerTodos()
        {
            var hijos = configuration.GetChildren().Select(x => $"{x.Key}: {x.Value}"); //obtener todos los datos
            return Ok (new {hijos});
        }

        [HttpGet("seccion_01")]
        public ActionResult<string> getSeccion_01()
        {
            var nombre = seccion_01.GetValue<string>("nombre");
            var edad = seccion_01.GetValue<int>("edad");

            return Ok(new { nombre, edad });
        }

        [HttpGet("seccion_02")]
        public ActionResult<string> getSeccion_02()
        {
            var nombre = seccion_02.GetValue<string>("nombre");
            var edad = seccion_02.GetValue<int>("edad");

            return Ok(new { nombre, edad });
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var opcion1 = configuration["apellido"];  //manera 1

            var opcion2 = configuration.GetValue<string>("apellido")!;  //manera 2

            return opcion2;

        }

        [HttpGet("secciones")]
        public ActionResult<string> getSeccion()
        {
            var opnion1 = configuration["ConnectionStrings:DefaultConnection"];
            
            var opcion2 = configuration.GetValue<string>("ConnectionStrings:DefaultConnection")!;

            var seccion = configuration.GetSection("ConnectionStrings");
            var opcion3 = seccion["DefaultConnection"];

            return opcion3!;
        }

    }
}
