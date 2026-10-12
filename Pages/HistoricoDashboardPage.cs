
using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages
{
    public class HistoricoDashboardPage
    {
        private readonly IPage _page;

        public HistoricoDashboardPage(IPage page)
        {
            _page = page;
        }

        public async Task AbrirHistoricoAsync()
        {
            var enlace = _page.Locator("a[href='/Dashboards/Historico']").First;

            await enlace.WaitForAsync(
                new() { State = WaitForSelectorState.Visible }
            );

            await enlace.ClickAsync();

            await _page.WaitForURLAsync("**/Dashboards/Historico");
        }

        public async Task ConsultarPeriodoAsync(
            string fechaInicio,
            string fechaFin)
        {
            await _page.Locator("input[name='fechaInicio']")
                .FillAsync(fechaInicio);

            await _page.Locator("input[name='fechaFin']")
                .FillAsync(fechaFin);

            await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Filtrar" }
            ).ClickAsync();

            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }

        public ILocator ObtenerTitulo()
        {
            return _page.GetByRole(
                AriaRole.Main
            ).GetByRole(
                AriaRole.Heading,
                new()
                {
                    Name = "Histórico de Órdenes de Trabajo",
                    Exact = false
                }
            );
        }

        public ILocator ObtenerGraficoOrdenes()
        {
            return _page.Locator("#chartOrdenes");
        }

        public ILocator ObtenerTarjetasResumen()
        {
            return _page.Locator(".dashboard-stats");
        }

        public ILocator ObtenerMensajeSinDatos()
        {
            return _page.GetByText(
                "No hay órdenes de trabajo registradas en el período seleccionado.",
                new() { Exact = true }
            );
        }

        public async Task EsperarGraficoAsync()
        {
            await _page.WaitForFunctionAsync(
                """
                () =>
                    typeof Chart !== 'undefined' &&
                    Chart.getChart('chartOrdenes') !== undefined
                """
            );
        }

        public async Task<int> ObtenerCantidadMesesAsync()
        {
            return await _page.EvaluateAsync<int>(
                "() => Chart.getChart('chartOrdenes').data.labels.length"
            );
        }

        public async Task<string> ObtenerFechaInicioAsync()
        {
            return await _page.Locator("input[name='fechaInicio']")
                .InputValueAsync();
        }

        public async Task<string> ObtenerFechaFinAsync()
        {
            return await _page.Locator("input[name='fechaFin']")
                .InputValueAsync();
        }
    }
}
