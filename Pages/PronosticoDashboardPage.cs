
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages
{
    // Una fila de la tabla de pronóstico: mes, cajas proyectadas y rango de confianza.
    public record FilaPronostico(
        string Mes,
        int CajasProyectadas,
        int LimiteInferior,
        int LimiteSuperior);

    public class PronosticoDashboardPage
    {
        private readonly IPage _page;

        public PronosticoDashboardPage(IPage page)
        {
            _page = page;
        }

        public async Task AbrirPronosticoAsync()
        {
            var enlace = _page.Locator("a[href='/Dashboards/Pronostico']").First;

            await enlace.WaitForAsync(
                new() { State = WaitForSelectorState.Visible }
            );

            await enlace.ClickAsync();

            await _page.WaitForURLAsync("**/Dashboards/Pronostico");
        }

        public ILocator ObtenerTitulo()
        {
            return _page.Locator("h2.content-title");
        }

        // Aparece cuando hay menos de 6 meses de historial
        public ILocator ObtenerMensajeDatosInsuficientes()
        {
            return _page.GetByText(
                "No hay suficientes datos históricos para generar un pronóstico confiable."
            );
        }

        // Aparece si el cálculo lanzó una excepción
        public ILocator ObtenerMensajeError()
        {
            return _page.GetByText("al generar el pronóstico");
        }

        public ILocator ObtenerResumenGeneracion()
        {
            return _page.Locator("div.badge-info").Filter(
                new() { HasText = "Pronóstico generado con base en" }
            );
        }

        public ILocator ObtenerGrafico()
        {
            return _page.Locator("#chartPronostico");
        }

        public ILocator ObtenerFilasPronostico()
        {
            return _page.Locator("table.data-table tbody tr");
        }

        // "Pronóstico generado con base en 12 meses de historial..." -> 12
        public async Task<int> ObtenerMesesDeHistorialAsync()
        {
            string texto = await ObtenerResumenGeneracion().InnerTextAsync();

            var coincidencia = Regex.Match(texto, @"base en (\d+) meses");

            if (!coincidencia.Success)
            {
                throw new InvalidOperationException(
                    $"No se pudo leer la cantidad de meses de historial en: '{texto.Trim()}'."
                );
            }

            return int.Parse(coincidencia.Groups[1].Value);
        }

        // Lee la tabla: Mes | Cajas proyectadas | "inferior — superior".
        // Los números se extraen con una expresión regular (pueden ser negativos).
        public async Task<List<FilaPronostico>> LeerFilasPronosticoAsync()
        {
            var filas = ObtenerFilasPronostico();
            int total = await filas.CountAsync();

            var resultado = new List<FilaPronostico>();

            for (int i = 0; i < total; i++)
            {
                var celdas = filas.Nth(i).Locator("td");

                string mes = (await celdas.Nth(0).InnerTextAsync()).Trim();
                string proyectadas = await celdas.Nth(1).InnerTextAsync();
                string rango = await celdas.Nth(2).InnerTextAsync();

                var numerosRango = Regex.Matches(rango, @"-?\d+");

                if (numerosRango.Count != 2)
                {
                    throw new InvalidOperationException(
                        $"El rango de confianza no tiene dos valores: '{rango.Trim()}'."
                    );
                }

                resultado.Add(new FilaPronostico(
                    mes,
                    int.Parse(Regex.Match(proyectadas, @"-?\d+").Value),
                    int.Parse(numerosRango[0].Value),
                    int.Parse(numerosRango[1].Value)));
            }

            return resultado;
        }

        // ---------------------------------------------------------
        // Gráfico (Chart.js)
        // ---------------------------------------------------------

        public async Task EsperarGraficoAsync()
        {
            await _page.WaitForFunctionAsync(
                """
                () =>
                    typeof Chart !== 'undefined' &&
                    Chart.getChart('chartPronostico') !== undefined
                """
            );
        }

        public async Task<string[]> ObtenerEtiquetasGraficoAsync()
        {
            return await _page.EvaluateAsync<string[]>(
                "() => Chart.getChart('chartPronostico').data.labels"
            );
        }

        public async Task<string[]> ObtenerNombresSeriesAsync()
        {
            return await _page.EvaluateAsync<string[]>(
                "() => Chart.getChart('chartPronostico').data.datasets.map(d => d.label)"
            );
        }

        // Últimos N valores de la serie "Proyección" (los pronosticados)
        public async Task<double[]> ObtenerUltimosValoresProyeccionAsync(int cantidad)
        {
            return await _page.EvaluateAsync<double[]>(
                """
                (n) => Chart.getChart('chartPronostico')
                    .data.datasets.find(d => d.label === 'Proyección')
                    .data.slice(-n)
                """,
                cantidad
            );
        }
    }
}
