using Microsoft.Extensions.Options;

namespace BibliotecaAPI
{
    public class PagosProcesamiento
    {
        private TarifaOpciones _tarifaOpciones;

        public PagosProcesamiento(IOptionsMonitor<TarifaOpciones> optionsMonitor)
        {
            _tarifaOpciones = optionsMonitor.CurrentValue;

            optionsMonitor.OnChange(nuevaTarifa =>
            {
                Console.WriteLine("tarifa catualizada");
                _tarifaOpciones = nuevaTarifa;
            });
        }

        public void ProcesarPago()
        {
            Console.WriteLine($"Procesando pago con tarifa de dia: {_tarifaOpciones.Dia} y tarifa de noche: {_tarifaOpciones.Noche}");
        }

        public TarifaOpciones ObtenerTarifas()
        {
            return _tarifaOpciones;
        }

    }
}
