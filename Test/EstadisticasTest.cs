
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
        [Category("Dashboard")]
        public async Task TC_DAS_004_Estadisticas_CantidadesYPorcentajes()
        {
            // 1. Iniciar sesión
            await IniciarSesionAsync();

            await CrearDatosParaDashboardsAsync();

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
            static int SoloEntero(string texto) =>
                int.Parse(System.Text.RegularExpressions.Regex.Replace(texto, @"\D", ""));

            var tarjetasStats = Page.Locator("section.dashboard-stats .stat-card");

            int cajasImportacion = SoloEntero(
                await tarjetasStats.Nth(0).Locator(".stat-info h3").InnerTextAsync());

            int cajasExportacion = SoloEntero(
                await tarjetasStats.Nth(1).Locator(".stat-info h3").InnerTextAsync());

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

            // 9. Verificar los porcentajes de las tarjetas de cajas
            static double Porcentaje(string textoTarjeta)
            {
                var m = System.Text.RegularExpressions.Regex.Match(
                    textoTarjeta, @"\((\d+(?:[.,]\d+)?)\s*%\)");

                Assert.That(m.Success, Is.True,
                    $"No se encontró un porcentaje en la tarjeta: '{textoTarjeta}'.");

                return double.Parse(
                    m.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            int totalCajas = cajasImportacion + cajasExportacion;

            Assert.That(totalCajas, Is.GreaterThan(0),
                "No hay cajas para calcular porcentajes.");

            double pctImportadas = Porcentaje(cajasImportadas);
            double pctExportadas = Porcentaje(cajasExportadas);

            Assert.That(
                pctImportadas + pctExportadas,
                Is.EqualTo(100).Within(0.2),
                "Los porcentajes de cajas no suman 100%.");

            Assert.That(
                pctImportadas,
                Is.EqualTo(cajasImportacion * 100.0 / totalCajas).Within(0.1),
                "El % de cajas importadas no corresponde a sus cantidades.");

            Assert.That(
                pctExportadas,
                Is.EqualTo(cajasExportacion * 100.0 / totalCajas).Within(0.1),
                "El % de cajas exportadas no corresponde a sus cantidades.");
        }
    }
}
