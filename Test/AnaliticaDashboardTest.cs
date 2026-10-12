
using Microsoft.Playwright;
using NUnit.Framework;
using Simulacion3_Calidad_Software.Utils;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;
using static Microsoft.Playwright.Assertions;

namespace WinMoversAutoTest.Tests
{
    [TestFixture]
    public class TC_DAS_001_VisualizarDashboard : TestBase
    {
        [Test]
        [Category("Dashboard")]
        public async Task TC_DAS_001_DashboardImportacionesExportaciones_Visible()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await IniciarSesionAsync();

            await CrearDatosParaDashboardsAsync();

            // 2. Abrir la analítica desde el menú
            var dashboardPage = new AnaliticaDashboardPage(Page);

            await dashboardPage.AbrirAnaliticaAsync();

            // 3. Verificar el título de la pantalla
            await Expect(dashboardPage.ObtenerTitulo())
                .ToBeVisibleAsync();

            // 4. Verificar que los gráficos estén presentes
            await Expect(dashboardPage.ObtenerGraficoCajas())
                .ToBeVisibleAsync();

            await Expect(dashboardPage.ObtenerGraficoKilos())
                .ToBeVisibleAsync();

            // 5. Comprobar que Chart.js haya generado ambos gráficos
            await dashboardPage.EsperarGraficosAsync();

            // 6. Verificar que existan meses representados en el gráfico
            int cantidadMeses =
                await dashboardPage.ObtenerCantidadMesesGraficadosAsync();

            Assert.That(
                cantidadMeses,
                Is.GreaterThan(0),
                "El gráfico de cajas no contiene meses para representar."
            );

            // 7. Verificar la sección de países
            await Expect(
                Page.GetByRole(
                    AriaRole.Heading,
                    new()
                    {
                        Name = "Top 5 países de origen (importaciones)",
                        Exact = true
                    }
                )
            ).ToBeVisibleAsync();

            await Expect(dashboardPage.ObtenerTablaPaises())
                .ToBeVisibleAsync();
        }
    }
}
