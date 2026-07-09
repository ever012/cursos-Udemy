using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.CompilerServices;

namespace BibliotecaAPI.Utilidades
{
    public static class ModelStateDictionaryExtensions
    {
        public static BadRequestObjectResult ContruirProblemDetail(this ModelStateDictionary modelState)
        {
            var problemDetails = new ValidationProblemDetails(modelState)
            {
                Title = "One or mode validation errors occured.",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(problemDetails);
        }
    }
}
