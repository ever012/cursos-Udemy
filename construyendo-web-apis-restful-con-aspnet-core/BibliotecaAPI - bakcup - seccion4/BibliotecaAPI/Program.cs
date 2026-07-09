using BibliotecaAPI;
using BibliotecaAPI.Controllers;
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// area de servicios
builder.Services.AddSingleton<IRepositorioValores,RepositorioValoresOracle>();

builder.Services.AddTransient<ServicioTransient>();
builder.Services.AddScoped<ServicioScoped>();
builder.Services.AddSingleton<ServicioSingleton>();



builder.Services.AddControllers().AddJsonOptions(opciones => 
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=defaultConnection"));

//fin de servicios

var app = builder.Build();

// area de middlewares
app.UseLogueaPeticion();
app.UseBloqueaPeticion();




/*app.Use(async (contexto, next) =>  //ejemplo middleware
{
    if (contexto.Request.Path == "/bloqueado")
    {
        contexto.Response.StatusCode = 403;
        await contexto.Response.WriteAsync("Acceso denegado");
    }
    else
        await next.Invoke();
});*/


app.MapControllers();

//fin de middlewares



app.Run();



