
using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// area de servicios

builder.Services.AddDataProtection();
builder.Services.AddDataProtection();


var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("mi-cabecera");
        //opcionesCORS.AllowAnyOrigin().AllowAnyMethod() /*get,post,put,patch,...*/.AllowAnyHeader();
    });
});

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));



builder.Services.AddControllers().AddNewtonsoftJson();
/*builder.Services.AddControllers().AddJsonOptions(opciones => 
    opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
*/

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=defaultConnection"));

builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<Usuario>>();
builder.Services.AddScoped<SignInManager<Usuario>>();  //SignInManager permite autenticar usuarios
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>(); //no necesito guardar estados por eso transient
builder.Services.AddTransient<IServicioHash, ServicioHash>();

builder.Services.AddHttpContextAccessor();  //permite acceder al contexto desde cualquier clase


builder.Services.AddAuthentication().AddJwtBearer(opciones => 
{
    opciones.MapInboundClaims = false; //para que no cambie el nombre de los claims automaticamente, lo que lo volvería confuso
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        //que tomaremos en cuenta a la hora de validar un token
        ValidateIssuer = false, //no validar el emisor del token
        ValidateAudience = false, //no validar la audiencia
        ValidateLifetime = true, //validamos la fecha de expiración del token
        ValidateIssuerSigningKey = true, //validar la firma o llave secreta del token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llavejwt"]!)), //podria ser "Jwt:Key"
        ClockSkew = TimeSpan.Zero //para no tener problemas de diferencias de tiempo entre el servidor y el cliente, se le da un margen de tiempo para que el token siga siendo válido
  
    };
    
});

//politica de autorizacion
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
});


//fin de servicios

var app = builder.Build();

// area de middlewares
app.Use(async (contexto, next) =>
{
    contexto.Response.Headers.Append("mi-cabecera", "valor");
    await next();
});
app.UseCors();




app.MapControllers();

//fin de middlewares



app.Run();



