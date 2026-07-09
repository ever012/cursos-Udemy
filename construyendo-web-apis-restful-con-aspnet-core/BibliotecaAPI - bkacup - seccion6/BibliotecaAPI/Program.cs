
using BibliotecaAPI;
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var diccionarioConfiguracion = new Dictionary<string, string>
{
    {
        "quien_soy", "Soy un diccionario en memoria"
    }
};

builder.Configuration.AddInMemoryCollection(diccionarioConfiguracion!);

// area de servicios
builder.Services.AddOptions<PersonaOpciones>()
    .Bind(builder.Configuration.GetSection(PersonaOpciones.Seccion))
    .ValidateDataAnnotations()  //para que funcionen los required del dataAnotations
    .ValidateOnStart();  //para validar al momento de levantar la aplicacion

builder.Services.AddOptions<TarifaOpciones>()
    .Bind(builder.Configuration.GetSection(TarifaOpciones.Seccion))
    .ValidateDataAnnotations()  //para que funcionen los required del dataAnotations
    .ValidateOnStart();  //para validar al momento de levantar la aplicacion


builder.Services.AddSingleton<PagosProcesamiento>();

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));



builder.Services.AddControllers().AddNewtonsoftJson();
/*builder.Services.AddControllers().AddJsonOptions(opciones => 
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
*/

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=defaultConnection"));

//fin de servicios

var app = builder.Build();

// area de middlewares





app.MapControllers();

//fin de middlewares



app.Run();



