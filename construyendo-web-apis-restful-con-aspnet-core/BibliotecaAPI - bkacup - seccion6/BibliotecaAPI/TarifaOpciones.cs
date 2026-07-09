using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace BibliotecaAPI
{
    public class TarifaOpciones
    {
        public const string Seccion = "tarifas";

        [Required]
        public decimal Dia { get; set; }
        [Required]
        public decimal Noche { get; set; }
    }
}
