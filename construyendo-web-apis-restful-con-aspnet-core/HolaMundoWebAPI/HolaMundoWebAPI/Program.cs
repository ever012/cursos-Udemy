

var builder = WebApplication.CreateBuilder(args);
var cadenaDeConexion = builder.Configuration.GetValue<string>("cadenaDeConexion");

// Inicio del area de servicios



// Fin del area de servicios

var app = builder.Build();

//inicio del area de los middlewares

app.MapGet("/", () => cadenaDeConexion);


//fin del area de los middlewares

app.Run();













