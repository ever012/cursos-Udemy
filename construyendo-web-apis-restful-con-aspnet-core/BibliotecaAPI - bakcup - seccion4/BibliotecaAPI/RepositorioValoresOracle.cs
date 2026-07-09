using BibliotecaAPI.Entidades;

namespace BibliotecaAPI
{
    public class RepositorioValoresOracle : IRepositorioValores
    {
        private List<Valor> _valores;
        public RepositorioValoresOracle()
        {
            _valores = new List<Valor>
            {
                new Valor { Id = 4, Nombre = "Valor Oracle 1" },
                new Valor { Id = 5, Nombre = "Valor Oracle 2" },
                new Valor { Id = 6, Nombre = "Valor Oracle 3" }
            };
        }

        public IEnumerable<Valor> ObtenerValores()
        {
            return _valores;
            /*return new List<Valor>
            {
                new Valor { Id = 4, Nombre = "Valor Oracle 1" },
                new Valor { Id = 5, Nombre = "Valor Oracle 2" },
                new Valor { Id = 6, Nombre = "Valor Oracle 3" }
            };*/
        }

        public void InsertarValor(Valor valor)
        {
            _valores.Add(valor);
        }




    }
}
