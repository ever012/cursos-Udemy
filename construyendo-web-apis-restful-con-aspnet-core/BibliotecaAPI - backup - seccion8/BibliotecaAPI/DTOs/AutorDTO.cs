namespace BibliotecaAPI.DTOs
{
    public class AutorDTO: RecursoDTO
    {
        public int Id { get; set; }
        public required string NameCompleto { get; set; }
        public string? Foto { get; set; }

    }
}
