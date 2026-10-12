
using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages
{
    public class AnaliticaDashboardPage
    {
        private readonly IPage _page;

        public AnaliticaDashboardPage(IPage page)
        {
            _page = page;
        }

        public async Task AbrirAnaliticaAsync()
        {
            var enlace = _page.Locator("a[href='/Dashboards/Analitica']").First;

            await enlace.WaitForAsync(
                new() { State = WaitForSelectorState.Visible }
            );

            await enlace.ClickAsync();

            await _page.WaitForURLAsync(
                "**/Dashboards/Analitica"
            );
        }

        public ILocator ObtenerTitulo()
        {
            return _page.GetByRole(
                AriaRole.Heading,
                new()
                {
                    Name = "Analítica de Importaciones y Exportaciones",
                    Exact = false
                }
            );
        }

        public ILocator ObtenerGraficoCajas()
        {
            return _page.Locator("#chartCajas");
        }

        public ILocator ObtenerGraficoKilos()
        {
            return _page.Locator("#chartKilos");
        }

        public ILocator ObtenerTablaPaises()
        {
            return _page.Locator("table.data-table");
        }

        public async Task EsperarGraficosAsync()
        {
            await _page.WaitForFunctionAsync(
                """
                () =>
                    typeof Chart !== 'undefined' &&
                    Chart.getChart('chartCajas') !== undefined &&
                    Chart.getChart('chartKilos') !== undefined
                """
            );
        }

        public async Task<int> ObtenerCantidadMesesGraficadosAsync()
        {
            return await _page.EvaluateAsync<int>(
                "() => Chart.getChart('chartCajas').data.labels.length"
            );
        }
    }
}
