
using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;
using static Microsoft.Playwright.Assertions;

namespace WinMoversAutoTest.Tests
{
    [TestFixture]
    public class TC_DAS_003_GenerarPronostico : TestBase
    {
        private const int MesesMinimos = 6;
        private const int MesesProyectados = 3;

        [Test]
        [Category("Dashboard")]
        public async Task TC_DAS_003_GenerarPronosticoDeTransporte()
        {
            // 1. Iniciar sesión
            await IniciarSesionAsync();

            // 2. Abrir el módulo de pronósticos desde el menú
            var pronosticoPage = new PronosticoDashboardPage(Page);

            await pronosticoPage.AbrirPronosticoAsync();

            await Expect(pronosticoPage.ObtenerTitulo())
                .ToContainTextAsync("Pronóstico de Transporte");

            await Expect(pronosticoPage.ObtenerTitulo())
                .ToContainTextAsync($"próximos {MesesProyectados} meses");

            // 3. Precondición del caso: suficiente historial de datos.
            // Con datos_demo.sql hay 12 meses. Si no hay datos suficientes el
            // resultado no se puede evaluar (no es un fallo del pronóstico).
            if (await pronosticoPage.ObtenerMensajeDatosInsuficientes().CountAsync() > 0)
            {
                Assert.Inconclusive(
                    $"No hay al menos {MesesMinimos} meses de importaciones/exportaciones. " +
                    "Cargue datos_demo.sql antes de ejecutar TC-DAS-003.");
            }

            // 4. El cálculo no debe haber fallado
            await Expect(pronosticoPage.ObtenerMensajeError()).ToHaveCountAsync(0);

            // 5. El sistema indica con cuántos meses de historial se generó
            await Expect(pronosticoPage.ObtenerResumenGeneracion()).ToBeVisibleAsync();

            await Expect(pronosticoPage.ObtenerResumenGeneracion())
                .ToContainTextAsync("ML.NET");

            int mesesHistorial = await pronosticoPage.ObtenerMesesDeHistorialAsync();

            Assert.That(
                mesesHistorial,
                Is.GreaterThanOrEqualTo(MesesMinimos),
                "El pronóstico se generó con menos meses de los requeridos."
            );

            // 6. La tabla muestra los próximos 3 meses
            await Expect(pronosticoPage.ObtenerFilasPronostico())
                .ToHaveCountAsync(MesesProyectados);

            var filas = await pronosticoPage.LeerFilasPronosticoAsync();

            Assert.That(
                filas.Select(f => f.Mes).Distinct().Count(),
                Is.EqualTo(MesesProyectados),
                "Los meses proyectados no son distintos entre sí."
            );

            foreach (var fila in filas)
            {
                Assert.That(
                    fila.CajasProyectadas,
                    Is.InRange(fila.LimiteInferior, fila.LimiteSuperior),
                    $"La proyección de {fila.Mes} queda fuera de su rango de confianza."
                );
            }

            // 7. El gráfico muestra histórico + proyección
            await Expect(pronosticoPage.ObtenerGrafico()).ToBeVisibleAsync();

            await pronosticoPage.EsperarGraficoAsync();

            string[] series = await pronosticoPage.ObtenerNombresSeriesAsync();

            Assert.That(
                series,
                Is.EquivalentTo(new[]
                {
                    "Límite superior", "Límite inferior", "Histórico", "Proyección"
                }),
                "El gráfico no contiene las series esperadas."
            );

            // 8. El gráfico es coherente con la tabla
            string[] etiquetas = await pronosticoPage.ObtenerEtiquetasGraficoAsync();

            Assert.That(
                etiquetas.Length,
                Is.EqualTo(mesesHistorial + MesesProyectados),
                "El gráfico debe tener los meses históricos más los 3 proyectados."
            );

            Assert.That(
                etiquetas.TakeLast(MesesProyectados),
                Is.EqualTo(filas.Select(f => f.Mes)),
                "Los meses proyectados del gráfico no coinciden con la tabla."
            );

            double[] valoresProyectados =
                await pronosticoPage.ObtenerUltimosValoresProyeccionAsync(MesesProyectados);

            Assert.That(
                valoresProyectados,
                Is.EqualTo(filas.Select(f => (double)f.CajasProyectadas)),
                "Las cajas proyectadas del gráfico no coinciden con la tabla."
            );
        }
    }
}
