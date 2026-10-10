using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;

namespace WinMoversAutoTest.Tests;

public class CotizacionesTests : TestBase
{

    // =====================================================
    // TC-COT-001: Generar una cotización con datos válidos
    // =====================================================

    [Test]
    [Category("Cotizaciones")]
    public async Task TC_COT_001_GenerarCotizacionConDatosValidos()
    {
        var cotizacionesPage = new CotizacionesPage(Page);

        // Datos de prueba
        var datos = new DatosCotizacion(
            NombreCliente: $"Cliente QA COT {DateTime.Now:HHmmssfff}",
            Origen: "San José, Costa Rica",
            Destino: "Puerto de Barcelona, España",
            CostoOrigen: 500m,
            CostoTramitesAduana: 300m,
            CostoFlete: 700m,
            CostoDestino: 200m,
            IncluyeSeguro: true,
            ValorDeclarado: 10000m);

        // Resultado esperado: subtotal 1700, seguro 3.5% de 10000 = 350, total 2050
        const decimal subtotal = 1700m;
        const decimal seguro = 350m;
        const decimal total = 2050m;

        // 1. Iniciar sesión
        await IniciarSesionAsync();

        // 2. Ir al módulo de cotizaciones
        await cotizacionesPage.IrACotizacionesAsync();
        await Page.WaitForURLAsync("**/Cotizacion");

        // 3. Seleccionar Nueva cotización
        await cotizacionesPage.IrANuevaCotizacionAsync();
        await Page.WaitForURLAsync("**/Cotizacion/Create");

        string numeroCotizacion =
            await cotizacionesPage.ObtenerNumeroEnFormularioAsync();

        // 4. Registrar cliente, servicio y datos del cálculo
        await cotizacionesPage.LlenarFormularioAsync(datos);

        // 5. La calculadora en pantalla muestra el desglose
        await cotizacionesPage.VerificarCalculoEnVivoAsync(
            subtotal, seguro, total);

        // 6. Guardar la cotización
        await cotizacionesPage.GuardarFormularioAsync();

        // 7. Redirige al detalle con mensaje de éxito
        await Page.WaitForURLAsync("**/Cotizacion/Details/*");
        await cotizacionesPage.VerificarMensajeCreacionAsync(numeroCotizacion);

        // 8. El detalle muestra el desglose calculado por el servidor
        await cotizacionesPage.VerificarDesgloseAsync(
            datos.CostoOrigen,
            datos.CostoTramitesAduana,
            datos.CostoFlete,
            datos.CostoDestino,
            subtotal,
            total,
            seguro);

        // 9. Estado inicial: Borrador
        await cotizacionesPage.VerificarEstadoAsync("Borrador");
    }

    // =====================================================
    // TC-COT-002: Convertir una cotización en orden de mudanza
    // =====================================================

    [Test]
    [Category("Cotizaciones")]
    public async Task TC_COT_002_ConvertirCotizacionEnOrden()
    {
        var cotizacionesPage = new CotizacionesPage(Page);
        var ordenesPage = new OrdenesPage(Page);

        await IniciarSesionAsync();

        // Precondición: una cotización creada por el propio test.
        // Nota: se convierte en estado Borrador; el sistema lo permite y solo avisa
        // que normalmente se convierte una vez aceptada (ver observaciones del caso).
        var (numeroCotizacion, urlDetalle, datos) =
            await CrearCotizacionPreviaAsync(cotizacionesPage);

        string numeroOT = $"QA{DateTime.Now:yyMMddHHmmssfff}";
        DateTime fechaServicio = DateTime.Today.AddDays(400 + Random.Shared.Next(0, 3000));

        // 1. Desde el detalle, seleccionar Convertir en orden
        await cotizacionesPage.IrAConvertirDesdeDetalleAsync();
        await Page.WaitForURLAsync("**/Cotizacion/Convertir/*");

        // 2. Confirmar la conversión con los datos de la orden
        await cotizacionesPage.ConvertirEnOrdenAsync(
            numeroOT,
            fechaServicio,
            "10:00");

        // 3. Redirige a la edición de la orden creada
        await Page.WaitForURLAsync("**/OrdenTrabajo/Edit/*");
        await cotizacionesPage.VerificarMensajeConversionAsync(
            numeroCotizacion,
            numeroOT);

        await ordenesPage.VerificarPaginaEditarOrdenAsync(numeroOT);

        // 4. La orden usa los datos de la cotización y nace en estado Pendiente
        await Assertions.Expect(Page.Locator("#NombreCliente"))
            .ToHaveValueAsync(datos.NombreCliente);

        await Assertions.Expect(Page.Locator("select#Estado"))
            .ToHaveValueAsync("Pendiente");

        // 5. Trazabilidad: el historial de la orden indica su origen
        await ordenesPage.IrAHistorialDesdeEdicionAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo/Historial/*");

        await ordenesPage.VerificarHistorialContieneAsync(
            $"Generada desde la cotización {numeroCotizacion}");

        // 6. La cotización queda como Convertida y enlazada a la orden
        await Page.GotoAsync(urlDetalle);

        await cotizacionesPage.VerificarEstadoAsync("Convertida");
        await cotizacionesPage.VerificarCotizacionConvertidaEnOrdenAsync(numeroOT);
    }

    // =====================================================
    // TC-COT-003: Editar una cotización guardada
    // =====================================================

    [Test]
    [Category("Cotizaciones")]
    public async Task TC_COT_003_EditarCotizacionGuardada()
    {
        var cotizacionesPage = new CotizacionesPage(Page);

        await IniciarSesionAsync();

        var (numeroCotizacion, _, datos) =
            await CrearCotizacionPreviaAsync(cotizacionesPage);

        // Datos modificados: el flete sube de 700 a 900 y se agrega una observación
        const decimal nuevoFlete = 900m;
        string observacion =
            $"Observación QA editada {DateTime.Now:yyyyMMddHHmmss}";

        decimal nuevoSubtotal = datos.CostoOrigen
            + datos.CostoTramitesAduana
            + nuevoFlete
            + datos.CostoDestino;

        // 1. Desde el detalle, seleccionar Editar
        await cotizacionesPage.IrAEditarDesdeDetalleAsync();
        await Page.WaitForURLAsync("**/Cotizacion/Edit/*");

        // 2. Modificar uno o más datos
        await cotizacionesPage.CambiarFleteAsync(nuevoFlete);
        await cotizacionesPage.EscribirObservacionesAsync(observacion);

        // 3. Guardar los cambios
        await cotizacionesPage.GuardarFormularioAsync();

        // 4. Redirige al detalle con mensaje y datos actualizados
        await Page.WaitForURLAsync("**/Cotizacion/Details/*");
        await cotizacionesPage.VerificarMensajeActualizacionAsync(numeroCotizacion);

        await cotizacionesPage.VerificarDesgloseAsync(
            datos.CostoOrigen,
            datos.CostoTramitesAduana,
            nuevoFlete,
            datos.CostoDestino,
            nuevoSubtotal,
            nuevoSubtotal);

        await cotizacionesPage.VerificarObservacionesAsync(observacion);
    }

    // =====================================================
    // TC-COT-004: Eliminar una cotización guardada
    // =====================================================

    [Test]
    [Category("Cotizaciones")]
    public async Task TC_COT_004_EliminarCotizacionGuardada()
    {
        var cotizacionesPage = new CotizacionesPage(Page);

        await IniciarSesionAsync();

        var (numeroCotizacion, _, _) =
            await CrearCotizacionPreviaAsync(cotizacionesPage);

        // 1. Ir al módulo y buscar la cotización
        await cotizacionesPage.IrACotizacionesAsync();
        await cotizacionesPage.BuscarPorTerminoAsync(numeroCotizacion);
        await cotizacionesPage.VerificarCotizacionEnListaAsync(numeroCotizacion);

        // 2. Eliminar (aceptando la confirmación del navegador)
        await cotizacionesPage.EliminarCotizacionAsync(numeroCotizacion);

        // 3. Redirige a la lista con mensaje de éxito
        await Page.WaitForURLAsync("**/Cotizacion");
        await cotizacionesPage.VerificarMensajeEliminacionAsync(numeroCotizacion);

        // 4. Ya no aparece al buscarla
        await cotizacionesPage.BuscarPorTerminoAsync(numeroCotizacion);
        await cotizacionesPage.VerificarSinResultadosAsync();
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    // Crea una cotización por la interfaz y termina en su página de detalle.
    // Así los casos 002, 003 y 004 no dependen de los datos que haya en la base.
    private async Task<(string Numero, string UrlDetalle, DatosCotizacion Datos)>
        CrearCotizacionPreviaAsync(CotizacionesPage cotizacionesPage)
    {
        var datos = new DatosCotizacion(
            NombreCliente: $"Cliente QA COT {DateTime.Now:HHmmssfff}",
            Origen: "San José, Costa Rica",
            Destino: "Puerto de Barcelona, España",
            CostoOrigen: 500m,
            CostoTramitesAduana: 300m,
            CostoFlete: 700m,
            CostoDestino: 200m);

        await cotizacionesPage.IrACotizacionesAsync();
        await cotizacionesPage.IrANuevaCotizacionAsync();
        await Page.WaitForURLAsync("**/Cotizacion/Create");

        string numero = await cotizacionesPage.ObtenerNumeroEnFormularioAsync();

        await cotizacionesPage.LlenarFormularioAsync(datos);
        await cotizacionesPage.GuardarFormularioAsync();

        await Page.WaitForURLAsync("**/Cotizacion/Details/*");
        await cotizacionesPage.VerificarMensajeCreacionAsync(numero);

        return (numero, Page.Url, datos);
    }
}
