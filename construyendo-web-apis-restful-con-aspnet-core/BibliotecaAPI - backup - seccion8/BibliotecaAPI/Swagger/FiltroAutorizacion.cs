using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BibliotecaAPI.Swagger
{
    public class FiltroAutorizacion : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //verifico que el endpoint tenga un Autorize, si no lo tiene 
            if(!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
            {
                return; //retorna porque no está protegido
            }

            //si tiene AllowAnonimous tambien lo va a permitir sin jwt
            if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return; //retorna porque no está protegido
            }


            operation.Security = new List<OpenApiSecurityRequirement> //creo una nueva lista de requisitos de seguridad
            {
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                }



            };

        }
    }
}
