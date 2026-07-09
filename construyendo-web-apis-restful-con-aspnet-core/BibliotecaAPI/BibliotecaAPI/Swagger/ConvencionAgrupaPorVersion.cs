using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BibliotecaAPI.Swagger
{
    public class ConvencionAgrupaPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            //ejemplo: "Controllers.V1"
            var nameSpaceDellControlador = controller.ControllerType.Namespace;
            var vesion = nameSpaceDellControlador!.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = vesion;


        }
    }
}
