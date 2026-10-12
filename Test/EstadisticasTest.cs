
using Microsoft.Playwright;
using NUnit.Framework;
using Simulacion3_Calidad_Software.Utils;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;
using static Microsoft.Playwright.Assertions;

namespace WinMoversAutoTest.Tests
{
    [TestFixture]
    public class TC_DAS_004_VisualizarEstadisticas : TestBase
    {
        [Test]
        public async Task TC_DAS_004_Estadisticas_CantidadesYPorcentajes()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await loginPage.NavigateAsync();

            await loginPage.LoginAsync(
                TestConfig.Email,
                TestConfig.Password
            );

            await Page.WaitForURLAsync("**https://localhost:7058/");

            // 2. Abrir el módulo de estadísticas
            var estadisticasPage =
                new EstadisticasDashboardPage(Page);

            await estadisticasPage.AbrirEstadisticasAsync();

            // 3. Verificar el encabezado
            await Expect(estadisticasPage.ObtenerTitulo())
                .ToBeVisibleAsync();

            // 4. Comprobar que hay datos para mostrar
            var mensajeSinDatos = Page.GetByText(
                "No hay datos de importaciones ni exportaciones disponibles todavía.",
                new() { Exact = true }
            );

            Assert.That(
                await mensajeSinDatos.CountAsync(),
                Is.EqualTo(0),
                "No hay importaciones ni exportaciones registradas. " +
                "No se puede validar TC-DAS-004."
            );

            // 5. Verificar las cuatro tarjetas de estadísticas
            var tarjetas = estadisticasPage.ObtenerTarjetasResumen();

            await Expect(tarjetas).ToHaveCountAsync(4);

            for (int i = 0; i < 4; i++)
            {
                await Expect(tarjetas.Nth(i)).ToBeVisibleAsync();
            }

            // 6. Comprobar los textos de las tarjetas
            string cajasImportadas =
                await estadisticasPage.ObtenerTextoTarjetaAsync(0);

            string cajasExportadas =
                await estadisticasPage.ObtenerTextoTarjetaAsync(1);

            string kilosImportados =
                await estadisticasPage.ObtenerTextoTarjetaAsync(2);

            string kilosExportados =
                await estadisticasPage.ObtenerTextoTarjetaAsync(3);

            Assert.That(
                cajasImportadas,
                Does.Contain("Cajas importadas")
            );

            Assert.That(
                cajasImportadas,
                Does.Contain("%")
            );

            Assert.That(
                cajasExportadas,
                Does.Contain("Cajas exportadas")
            );

            Assert.That(
                cajasExportadas,
                Does.Contain("%")
            );

            Assert.That(
                kilosImportados,
                Does.Contain("Kilos importados")
            );

            Assert.That(
                kilosExportados,
                Does.Contain("Kilos exportados")
            );

            // 7. Verificar el gráfico de distribución de cajas
            await Expect(estadisticasPage.ObtenerGraficoDona())
                .ToBeVisibleAsync();

            await estadisticasPage.EsperarGraficoAsync();

            int[] valoresGrafico =
                await estadisticasPage.ObtenerValoresGraficoAsync();

            Assert.That(
                valoresGrafico,
                Has.Length.EqualTo(2),
                "El gráfico debe representar importaciones y exportaciones."
            );

            // 8. Comparar el gráfico con las cantidades de las tarjetas
            int cajasImportacion = await Page
                .Locator("section.dashboard-stats .stat-card")
                .Nth(0)
                .Locator(".stat-info h3")
                .InnerTextAsync()
                .ContinueWith(t => int.Parse(t.Result.Trim()));

            int cajasExportacion = await Page
                .Locator("section.dashboard-stats .stat-card")
                .Nth(1)
                .Locator(".stat-info h3")
                .InnerTextAsync()
                .ContinueWith(t => int.Parse(t.Result.Trim()));

            Assert.That(
                valoresGrafico[0],
                Is.EqualTo(cajasImportacion),
                "El gráfico no coincide con las cajas importadas."
            );

            Assert.That(
                valoresGrafico[1],
                Is.EqualTo(cajasExportacion),
                "El gráfico no coincide con las cajas exportadas."
            );
        }
    }
}
