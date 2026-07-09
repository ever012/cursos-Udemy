
using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// area de servicios

/*builder.Services.AddOutputCache(opciones =>  //usar oputput catch de manerea normal sin redis 
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);


});*/
builder.Services.AddStackExchangeRedisOutputCache(opciones =>   //output cache versin para redis
{
    opciones.Configuration = builder.Configuration.GetConnectionString("redis");

});



builder.Services.AddDataProtection();
builder.Services.AddDataProtection();


var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("cantidad-total-registros");
        //opcionesCORS.AllowAnyOrigin().AllowAnyMethod() /*get,post,put,patch,...*/.AllowAnyHeader();
    });
});

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));



builder.Services.AddControllers(opciones =>
{
    opciones.Filters.Add<FiltroTiempoEjecucion>();
    opciones.Conventions.Add(new ConvencionAgrupaPorVersion());
}).AddNewtonsoftJson();
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
//builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosAzure>();
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
builder.Services.AddScoped<MiFiltroDeAccion>();
builder.Services.AddScoped<FiltroValidacionLibro>();
builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IServicioAutores, BibliotecaAPI.Servicios.V1.ServicioAutores>();
builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IGeneradorEnlaces, BibliotecaAPI.Servicios.V1.GeneradorEnlaces>();
builder.Services.AddScoped<HATEOASAutorAttribute>();
builder.Services.AddScoped<HATEOASAutoresAttribute>();


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

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Version = "v1",
        Title = "Biblioteca API",
        Description = "Este es un web api para trabajar con datos de autores y libros",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Email = "ever@gmail.com",
            Name = "Ever Castellon",
            Url = new Uri("https://ever.com")
        },
        License = new Microsoft.OpenApi.OpenApiLicense{
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    opciones.SwaggerDoc("v2", new Microsoft.OpenApi.OpenApiInfo
    {
        Version = "v2",
        Title = "Biblioteca API",
        Description = "Este es un web api para trabajar con datos de autores y libros",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Email = "ever@gmail.com",
            Name = "Ever Castellon",
            Url = new Uri("https://ever.com")
        },
        License = new Microsoft.OpenApi.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    //configurar un jwt
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme  
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    opciones.OperationFilter<FiltroAutorizacion>(); //filtro para agregar el candado a los endpoints que tengan autorizacion

    /*opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement //esto le coloca un candadito para solicitar jwt en swagger
    {
            [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
    });*/

});


//fin de servicios

var app = builder.Build();

// area de middlewares

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var excepcion = exceptionHandlerFeature?.Error!;

    var error = new Error()
    {
        MensajeDeError = excepcion.Message,
        StrackTrace = excepcion.StackTrace,
        Fecha = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new 
    {
        tipo = "error", 
        mensaje = "Ha ocurrido un error inesperado", 
        estatus = 500
    }).ExecuteAsync(context);
}));

app.UseSwagger();
app.UseSwaggerUI(opciones => 
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json","Biblioteca API V1"); //esto es apra que se puedan tenr varias versiones y solo se vayan seleccionando  la version de endpoints necesarios
    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Biblioteca API V2");
});

app.UseStaticFiles(); //le indico que quiero servir archivos estaticos

app.UseCors();

app.UseOutputCache();



app.MapControllers();

//fin de middlewares



app.Run();



