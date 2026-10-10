using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace WinMoversAutoTest.Pages;

// Datos mínimos para llenar el formulario de cotización.
public record DatosCotizacion(
    string NombreCliente,
    string Origen,
    string Destino,
    decimal CostoOrigen,
    decimal CostoTramitesAduana,
    decimal CostoFlete,
    decimal CostoDestino,
    bool IncluyeSeguro = false,
    decimal ValorDeclarado = 0);

public class CotizacionesPage
{
    private readonly IPage _page;

    public CotizacionesPage(IPage page)
    {
        _page = page;
    }

    // =====================================================
    // NAVEGACIÓN
    // =====================================================

    public async Task IrACotizacionesAsync()
    {
        await _page.GotoAsync("/Cotizacion");
    }

    public async Task IrANuevaCotizacionAsync()
    {
        await _page
            .Locator("a.btn-primary[href*='/Cotizacion/Create']")
            .ClickAsync();
    }

    public async Task IrAEditarDesdeDetalleAsync()
    {
        await _page
            .Locator("a[href*='/Cotizacion/Edit/']")
            .ClickAsync();
    }

    public async Task IrAConvertirDesdeDetalleAsync()
    {
        await _page
            .Locator("a[href*='/Cotizacion/Convertir/']")
            .ClickAsync();
    }

    // =====================================================
    // FORMULARIO (Crear / Editar)
    // =====================================================

    // El número lo genera el sistema y se muestra en el título del formulario.
    public async Task<string> ObtenerNumeroEnFormularioAsync()
    {
        var texto = await _page
            .Locator("h2.content-title .numero-cotizacion")
            .InnerTextAsync();

        return texto.Trim();
    }

    public async Task LlenarFormularioAsync(DatosCotizacion d)
    {
        await _page.Locator("#txtNombreCliente").FillAsync(d.NombreCliente);
        await _page.Locator("#Origen").FillAsync(d.Origen);
        await _page.Locator("#Destino").FillAsync(d.Destino);

        await _page.Locator("#CostoOrigen").FillAsync(Num(d.CostoOrigen));
        await _page.Locator("#CostoTramitesAduana").FillAsync(Num(d.CostoTramitesAduana));
        await _page.Locator("#CostoFlete").FillAsync(Num(d.CostoFlete));
        await _page.Locator("#CostoDestino").FillAsync(Num(d.CostoDestino));

        if (d.IncluyeSeguro)
        {
            // Los campos del seguro están ocultos hasta marcar el check
            await _page.Locator("#chkSeguro").CheckAsync();
            await _page.Locator("#ValorDeclarado").FillAsync(Num(d.ValorDeclarado));
        }
    }

    public async Task CambiarFleteAsync(decimal costoFlete)
    {
        await _page.Locator("#CostoFlete").FillAsync(Num(costoFlete));
    }

    public async Task EscribirObservacionesAsync(string texto)
    {
        await _page.Locator("#Observaciones").FillAsync(texto);
    }

    // Crear y Editar usan el mismo formulario (#formCotizacion) con un único botón submit.
    public async Task GuardarFormularioAsync()
    {
        await _page
            .Locator("form#formCotizacion button[type='submit']")
            .ClickAsync();
    }

    // =====================================================
    // CÁLCULO EN VIVO (JavaScript del formulario)
    // =====================================================

    public async Task VerificarCalculoEnVivoAsync(
        decimal subtotal,
        decimal montoSeguro,
        decimal total)
    {
        await Assertions.Expect(_page.Locator("#outSubtotal"))
            .ToHaveTextAsync(Usd(subtotal));

        await Assertions.Expect(_page.Locator("#outSeguro"))
            .ToHaveTextAsync(Usd(montoSeguro));

        await Assertions.Expect(_page.Locator("#outTotal"))
            .ToHaveTextAsync(Usd(total));
    }

    // =====================================================
    // DETALLE
    // =====================================================

    public async Task VerificarMensajeCreacionAsync(string numeroCotizacion)
    {
        await Assertions.Expect(
            _page.GetByText($"Cotización {numeroCotizacion} creada por")
        ).ToBeVisibleAsync();
    }

    public async Task VerificarMensajeActualizacionAsync(string numeroCotizacion)
    {
        await Assertions.Expect(
            _page.GetByText($"Cotización {numeroCotizacion} actualizada.")
        ).ToBeVisibleAsync();
    }

    public async Task VerificarMensajeConversionAsync(
        string numeroCotizacion,
        string numeroOT)
    {
        await Assertions.Expect(
            _page.GetByText(
                $"Cotización {numeroCotizacion} convertida en la orden {numeroOT}.",
                new() { Exact = true })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarEstadoAsync(string estado)
    {
        await Assertions.Expect(
            _page.Locator("h2.content-title span.badge").First
        ).ToContainTextAsync(estado);
    }

    public async Task VerificarCotizacionConvertidaEnOrdenAsync(string numeroOT)
    {
        await Assertions.Expect(
            _page.Locator("div.alert-success")
                .Filter(new LocatorFilterOptions { HasText = numeroOT })
                .First
        ).ToBeVisibleAsync();
    }

    public async Task VerificarObservacionesAsync(string texto)
    {
        await Assertions.Expect(
            _page.Locator("div.bloque-texto")
                .Filter(new LocatorFilterOptions { HasText = texto })
        ).ToBeVisibleAsync();
    }

    // El servidor da formato a los montos según su cultura (coma, punto,
    // espacio...), así que se comparan solo los dígitos: 1,700.00 / 1.700,00 -> "170000".
    public async Task VerificarDesgloseAsync(
        decimal costoOrigen,
        decimal costoTramitesAduana,
        decimal costoFlete,
        decimal costoDestino,
        decimal subtotal,
        decimal tarifaTotal,
        decimal? montoSeguro = null)
    {
        await VerificarMontoAsync("Servicios en origen", costoOrigen);
        await VerificarMontoAsync("Trámites de aduana", costoTramitesAduana);
        await VerificarMontoAsync("Flete marítimo internacional", costoFlete);
        await VerificarMontoAsync("Servicios en destino", costoDestino);
        await VerificarMontoAsync("Subtotal", subtotal);

        if (montoSeguro.HasValue)
            await VerificarMontoAsync("Seguro", montoSeguro.Value);

        await VerificarMontoAsync("TARIFA TOTAL", tarifaTotal);
    }

    private async Task VerificarMontoAsync(string textoFila, decimal esperado)
    {
        var fila = _page
            .Locator("table.tabla-rubros tr")
            .Filter(new LocatorFilterOptions { HasText = textoFila })
            .First;

        await Assertions.Expect(fila).ToBeVisibleAsync();

        var textoMonto = await fila.Locator("td.col-monto").Last.InnerTextAsync();

        var esperadoEnCentavos = ((long)Math.Round(esperado * 100)).ToString();

        Assert.That(
            Regex.Replace(textoMonto, @"\D", ""),
            Is.EqualTo(esperadoEnCentavos),
            $"Monto incorrecto en la fila '{textoFila}' (se leyó '{textoMonto}').");
    }

    // =====================================================
    // CONVERTIR EN ORDEN
    // =====================================================

    public async Task ConvertirEnOrdenAsync(
        string numeroOT,
        DateTime fechaServicio,
        string hora)
    {
        await _page.Locator("#numeroOT").FillAsync(numeroOT);

        await _page.Locator("#fechaServicio")
            .FillAsync(fechaServicio.ToString("yyyy-MM-dd"));

        await _page.Locator("#hora").FillAsync(hora);

        await _page
            .GetByRole(AriaRole.Button, new() { Name = "Crear Orden de Mudanza" })
            .ClickAsync();
    }

    // =====================================================
    // LISTA: BUSCAR Y ELIMINAR
    // =====================================================

    public async Task BuscarPorTerminoAsync(string termino)
    {
        await _page.Locator("#Termino").FillAsync(termino);

        await _page
            .Locator("form.filtros-cotizacion button[type='submit']")
            .ClickAsync();
    }

    public async Task VerificarCotizacionEnListaAsync(string numeroCotizacion)
    {
        await Assertions.Expect(FilaDeCotizacion(numeroCotizacion))
            .ToBeVisibleAsync();
    }

    public async Task EliminarCotizacionAsync(string numeroCotizacion)
    {
        var fila = FilaDeCotizacion(numeroCotizacion);

        await Assertions.Expect(fila).ToBeVisibleAsync();

        // El botón de eliminar usa confirm() del navegador: hay que aceptarlo,
        // porque Playwright lo cierra (cancela) por defecto.
        _page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        await fila.Locator("button.btn-icon.delete").ClickAsync();
    }

    public async Task VerificarMensajeEliminacionAsync(string numeroCotizacion)
    {
        await Assertions.Expect(
            _page.GetByText($"Cotización {numeroCotizacion} eliminada.",
                new() { Exact = true })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarSinResultadosAsync()
    {
        await Assertions.Expect(
            _page.GetByText("No hay cotizaciones que coincidan.")
        ).ToBeVisibleAsync();
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    private ILocator FilaDeCotizacion(string numeroCotizacion)
    {
        return _page
            .Locator("table.data-table tbody tr")
            .Filter(new LocatorFilterOptions { HasText = numeroCotizacion });
    }

    private static string Num(decimal valor) =>
        valor.ToString(CultureInfo.InvariantCulture);

    // La calculadora en vivo siempre formatea en en-US: "US$1,700.00"
    private static string Usd(decimal valor) =>
        "US$" + valor.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
}
