namespace BibliotecaAPI.DTOs
{
    public record PaginacionDTO(int Pagina = 1, int RecordsPorPagina = 10)
    {
        private const int CantidadMaximaRecordsPorPagina = 50;

        public int Pagina { get; init; } = Math.Max(1, Pagina); //para evitar errores como que el cliente quiere ver pagina -1, ya que Math.Max devuelve el valor mas gande, asi que el minimo seria 1
        public int RecordsPorPagina { get; init; } = Math.Clamp(RecordsPorPagina, 1, CantidadMaximaRecordsPorPagina); //evitar que la cantidad de decimales, 0.5,1.5,etc.
    }
}
