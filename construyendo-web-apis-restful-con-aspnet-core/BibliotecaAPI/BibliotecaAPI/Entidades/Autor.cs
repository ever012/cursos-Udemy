using BibliotecaAPI.Validaciones;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Autor 
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="El campo {0} es requerido")]
        [StringLength(150,ErrorMessage ="El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayusculas]
        public required string Nombres { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayusculas]
        public required string Apellidos { get; set; }
        [StringLength(20, ErrorMessage ="El campo {0} debe tener {1} caracteres o menos")]
        public string? identificacion { get; set; }

        [Unicode(false)] //es para ser mas eficiente y que en la DB cree un VARCHAR y no un NVARCHAR
        public string? Foto { get; set; }
        public List<AutorLibro> Libros { get; set; } = [];
        


    }
}
