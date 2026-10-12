
using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages
{
    public class EstadisticasDashboardPage
    {
        private readonly IPage _page;

        public EstadisticasDashboardPage(IPage page)
        {
            _page = page;
        }

        public async Task AbrirEstadisticasAsync()
        {
            var enlace = _page.Locator(
                "a[href='/Dashboards/Estadisticas']"
            );

            await enlace.WaitForAsync(
                new() { State = WaitForSelectorState.Visible }
            );

            await enlace.ClickAsync();

            await _page.WaitForURLAsync(
                "**/Dashboards/Estadisticas"
            );
        }

        public ILocator ObtenerTitulo()
        {
            return _page.GetByRole(AriaRole.Heading, new()
            {
                NameRegex = new System.Text.RegularExpressions.Regex(
                    "Estadísticas de Objetos Transportados"
                )
            });
        }

        public ILocator ObtenerTarjetasResumen()
        {
            return _page.Locator("section.dashboard-stats .stat-card");
        }

        public ILocator ObtenerGraficoDona()
        {
            return _page.Locator("#chartDona");
        }

        public async Task EsperarGraficoAsync()
        {
            await _page.WaitForFunctionAsync(
                """
                () =>
                    typeof Chart !== 'undefined' &&
                    Chart.getChart('chartDona') !== undefined
                """
            );
        }

        public async Task<int[]> ObtenerValoresGraficoAsync()
        {
            return await _page.EvaluateAsync<int[]>(
                """
                () => Chart.getChart('chartDona')
                    .data.datasets[0].data
                """
            );
        }

        public async Task<string> ObtenerTextoTarjetaAsync(int indice)
        {
            var tarjeta = ObtenerTarjetasResumen().Nth(indice);

            return (await tarjeta.Locator(".stat-info").InnerTextAsync())
                .Trim();
        }

        public ILocator ObtenerTablaServicios()
        {
            return _page.Locator("table.data-table");
        }
    }
}
