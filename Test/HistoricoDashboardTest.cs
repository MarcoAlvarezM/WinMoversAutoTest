
using Microsoft.Playwright;
using NUnit.Framework;
using Simulacion3_Calidad_Software.Utils;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;
using static Microsoft.Playwright.Assertions;

namespace WinMoversAutoTest.Tests
{
    [TestFixture]
    public class TC_DAS_002_ConsultarDatosHistoricos : TestBase
    {
        [Test]
        public async Task TC_DAS_002_ConsultarHistorico_PorPeriodo()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await loginPage.NavigateAsync();

            await loginPage.LoginAsync(
                TestConfig.Email,
                TestConfig.Password
            );

            await Page.WaitForURLAsync("**https://localhost:7058/");

            // 2. Abrir el histórico desde el menú
            var historicoPage = new HistoricoDashboardPage(Page);

            await historicoPage.AbrirHistoricoAsync();

            await Expect(historicoPage.ObtenerTitulo())
                .ToBeVisibleAsync();

            // 3. Seleccionar un periodo amplio
            const string fechaInicio = "2000-01-01";
            const string fechaFin = "2099-12-31";

            await historicoPage.ConsultarPeriodoAsync(
                fechaInicio,
                fechaFin
            );

            // 4. Comprobar que el sistema conservó el periodo consultado
            Assert.That(
                await historicoPage.ObtenerFechaInicioAsync(),
                Is.EqualTo(fechaInicio),
                "La fecha inicial consultada no coincide."
            );

            Assert.That(
                await historicoPage.ObtenerFechaFinAsync(),
                Is.EqualTo(fechaFin),
                "La fecha final consultada no coincide."
            );

            // 5. Verificar que se presentan datos históricos
            await Expect(historicoPage.ObtenerTarjetasResumen())
                .ToBeVisibleAsync();

            await Expect(
                Page.GetByText(
                    "Órdenes en el período",
                    new() { Exact = true }
                )
            ).ToBeVisibleAsync();

            await Expect(
                Page.GetByText(
                    "Completadas",
                    new() { Exact = true }
                )
            ).ToBeVisibleAsync();

            // 6. Comprobar que el gráfico contiene datos
            await Expect(historicoPage.ObtenerGraficoOrdenes())
                .ToBeVisibleAsync();

            await historicoPage.EsperarGraficoAsync();

            int cantidadMeses =
                await historicoPage.ObtenerCantidadMesesAsync();

            Assert.That(
                cantidadMeses,
                Is.GreaterThan(0),
                "El gráfico histórico no contiene meses para representar."
            );
        }
    }
}
