using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages;

public class OrdenesPage
{
    private readonly IPage _page;

    public OrdenesPage(IPage page)
    {
        _page = page;
    }

    // =====================================================
    // NAVEGACIÓN
    // =====================================================

    public async Task IrAOrdenesAsync()
    {
        await _page.GotoAsync("/OrdenTrabajo");
    }

    public async Task IrANuevaOrdenAsync()
    {
        // En el Index el botón "nueva orden" apunta a /OrdenTrabajo/Create
        await _page
            .Locator("a.btn-primary[href*='/OrdenTrabajo/Create']")
            .ClickAsync();
    }

    public async Task IrAEditarOrdenAsync(string numeroOT)
    {
        var fila = FilaDeOrden(numeroOT);

        await Assertions.Expect(fila).ToBeVisibleAsync();

        await fila
            .Locator("a[href*='/OrdenTrabajo/Edit/']")
            .ClickAsync();
    }

    public async Task IrAHistorialDesdeEdicionAsync()
    {
        await _page
            .Locator("a[href*='/OrdenTrabajo/Historial/']")
            .ClickAsync();
    }

    public async Task IrANotasDesdeEdicionAsync()
    {
        await _page
            .Locator("a[href*='/OrdenTrabajo/Notas/']")
            .ClickAsync();
    }

    // =====================================================
    // CREAR ORDEN
    // =====================================================

    public async Task CrearOrdenAsync(
        string numeroOT,
        DateTime fechaServicio,
        string hora,
        string nombreCliente,
        string telefonoCelular,
        string direccionOrigen,
        string direccionDestino,
        string detalleServicio,
        string facturarA,
        string hechoPor)
    {
        await _page.Locator("#NumeroOT").FillAsync(numeroOT);

        await _page.Locator("#FechaServicio")
            .FillAsync(fechaServicio.ToString("yyyy-MM-dd"));

        await _page.Locator("#Fecha")
            .FillAsync(DateTime.Today.ToString("yyyy-MM-dd"));

        await _page.Locator("#Hora").FillAsync(hora);

        await _page.Locator("#NombreCliente").FillAsync(nombreCliente);
        await _page.Locator("#TelefonoCelular").FillAsync(telefonoCelular);

        await _page.Locator("#DireccionOrigen").FillAsync(direccionOrigen);
        await _page.Locator("#DireccionDestino").FillAsync(direccionDestino);
        await _page.Locator("#DetalleServicio").FillAsync(detalleServicio);

        await _page.Locator("#FacturarA").FillAsync(facturarA);
        await _page.Locator("#HechoPor").FillAsync(hechoPor);

        await GuardarFormularioAsync();
    }

    // =====================================================
    // EDITAR ORDEN
    // =====================================================

    public async Task VerificarPaginaEditarOrdenAsync(string numeroOT)
    {
        await Assertions.Expect(
            _page.Locator("h2.content-title")
        ).ToContainTextAsync($"Editar Orden — {numeroOT}");
    }

    public async Task AsignarFechaServicioAsync(DateTime fechaServicio)
    {
        await _page.Locator("#FechaServicio")
            .FillAsync(fechaServicio.ToString("yyyy-MM-dd"));
    }

    public async Task CambiarEstadoAsync(string estado)
    {
        await _page.Locator("select#Estado")
            .SelectOptionAsync(new SelectOptionValue { Label = estado });
    }

    // El formulario de Crear y el de Editar tienen un único botón submit
    // (el de subir archivos es type="button").
    public async Task GuardarFormularioAsync()
    {
        await _page
            .Locator("form.form-card button[type='submit']")
            .ClickAsync();
    }

    // =====================================================
    // VERIFICACIONES
    // =====================================================

    public async Task VerificarMensajeCreacionAsync()
    {
        await Assertions.Expect(
            _page.GetByText("Orden de Trabajo creada correctamente.",
                new() { Exact = true })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarMensajeActualizacionAsync()
    {
        await Assertions.Expect(
            _page.GetByText("Orden de Trabajo actualizada correctamente.",
                new() { Exact = true })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarOrdenEnListaAsync(
        string numeroOT,
        string estado,
        DateTime? fechaServicio = null)
    {
        var fila = FilaDeOrden(numeroOT);

        await Assertions.Expect(fila).ToBeVisibleAsync();
        await Assertions.Expect(fila).ToContainTextAsync(estado);

        if (fechaServicio.HasValue)
        {
            // El Index muestra la fecha como dd/MM/yyyy
            await Assertions.Expect(fila)
                .ToContainTextAsync(fechaServicio.Value.ToString("dd/MM/yyyy"));
        }
    }

    // Historial de cambios (auditoría): la tabla muestra Campo, Valor anterior,
    // Valor nuevo, Usuario y Fecha del cambio.
    public async Task VerificarHistorialContieneAsync(params string[] textos)
    {
        foreach (var texto in textos)
        {
            await Assertions.Expect(
                _page.Locator("table tbody")
            ).ToContainTextAsync(texto);
        }
    }

    // =====================================================
    // NOTAS
    // =====================================================

    public async Task AgregarNotaAsync(string contenido)
    {
        // Se apunta al formulario de "Nueva nota": las notas ya existentes
        // también tienen un textarea "contenido" (oculto) para editarlas.
        await _page
            .Locator("form[action*='AgregarNota'] textarea[name='contenido']")
            .FillAsync(contenido);

        await _page
            .Locator("form[action*='AgregarNota'] button[type='submit']")
            .ClickAsync();
    }

    public async Task VerificarMensajeNotaAgregadaAsync()
    {
        await Assertions.Expect(
            _page.Locator("div.alert-success")
        ).ToContainTextAsync("Nota agregada correctamente.");
    }

    public async Task VerificarNotaEnHistorialAsync(string contenido)
    {
        var tarjetaNota = _page
            .Locator("div[id^='nota-']")
            .Filter(new LocatorFilterOptions { HasText = contenido });

        await Assertions.Expect(tarjetaNota).ToBeVisibleAsync();

        // Debe incluir fecha y hora (dd/MM/yyyy HH:mm). El servidor guarda la hora
        // con su propio reloj (en AWS suele ser UTC, 6 h adelantado respecto a
        // Costa Rica), así que se acepta la fecha de hoy según la hora local o UTC.
        var fechasAceptadas = new[] { DateTime.Now, DateTime.UtcNow }
            .Select(f => f.ToString("dd/MM/yyyy"))
            .Distinct();

        var patronFecha = new Regex(
            $@"({string.Join("|", fechasAceptadas)}) \d{{2}}:\d{{2}}");

        await Assertions.Expect(tarjetaNota)
            .ToContainTextAsync(patronFecha);

        await Assertions.Expect(tarjetaNota)
            .Not.ToContainTextAsync("Sistema");
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    private ILocator FilaDeOrden(string numeroOT)
    {
        return _page
            .Locator("table.data-table tbody tr")
            .Filter(new LocatorFilterOptions { HasText = numeroOT });
    }
}
