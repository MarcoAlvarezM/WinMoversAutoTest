using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;

namespace WinMoversAutoTest.Pages;

// Importación y Exportación tienen pantallas casi idénticas (Index, Create, Edit,
// Checklist), así que comparten esta clase. Solo cambia el módulo en la URL y el
// campo País, que existe únicamente en Importación.
public class EmbarquesPage
{
    private readonly IPage _page;
    private readonly string _modulo; // "Importacion" | "Exportacion"

    public EmbarquesPage(IPage page, string modulo)
    {
        _page = page;
        _modulo = modulo;
    }

    public static EmbarquesPage Importaciones(IPage page) => new(page, "Importacion");

    public static EmbarquesPage Exportaciones(IPage page) => new(page, "Exportacion");

    public bool EsImportacion => _modulo == "Importacion";

    private string Nombre => EsImportacion ? "Importación" : "Exportación";

    // =====================================================
    // NAVEGACIÓN
    // =====================================================

    public async Task IrAListadoAsync()
    {
        await _page.GotoAsync($"/{_modulo}");
    }

    public async Task IrANuevoEmbarqueAsync()
    {
        await _page
            .Locator($"a.btn-primary[href*='/{_modulo}/Create']")
            .ClickAsync();
    }

    public async Task IrAEditarAsync(string referencia)
    {
        var fila = FilaDeEmbarque(referencia);

        await Assertions.Expect(fila).ToBeVisibleAsync();

        await fila.Locator("a[title='Editar']").ClickAsync();
    }

    public async Task IrAChecklistAsync(string referencia)
    {
        var fila = FilaDeEmbarque(referencia);

        await Assertions.Expect(fila).ToBeVisibleAsync();

        await fila.Locator("a[title='Gestionar Checklist']").ClickAsync();
    }

    // =====================================================
    // CREAR / EDITAR
    // =====================================================

    // La referencia debe ser el número de una O.T. que ya exista.
    // El país solo se llena en Importación.
    public async Task CrearEmbarqueAsync(
        string cliente,
        string referencia,
        int cajas,
        decimal kilos,
        DateTime fecha,
        string observaciones,
        string? pais = null)
    {
        await _page.Locator("#NombreCliente").FillAsync(cliente);

        if (EsImportacion && pais != null)
            await _page.Locator("#Pais").FillAsync(pais);

        await _page.Locator("#Referencia").FillAsync(referencia);
        await _page.Locator("#Cajas").FillAsync(cajas.ToString());
        await _page.Locator("#Kilos").FillAsync(Num(kilos));
        await _page.Locator("#Fecha").FillAsync(fecha.ToString("yyyy-MM-dd"));
        await _page.Locator("#Observaciones").FillAsync(observaciones);

        await GuardarFormularioAsync();
    }

    public async Task VerificarPaginaEditarAsync()
    {
        await Assertions.Expect(
            _page.Locator("h2.content-title")
        ).ToContainTextAsync($"Editar Embarque de {Nombre}");
    }

    public async Task EditarCajasYKilosAsync(int cajas, decimal kilos)
    {
        await _page.Locator("#Cajas").FillAsync(cajas.ToString());
        await _page.Locator("#Kilos").FillAsync(Num(kilos));

        await GuardarFormularioAsync();
    }

    public async Task EditarInformacionGeneralAsync(
        string cliente,
        DateTime fecha,
        string observaciones)
    {
        await _page.Locator("#NombreCliente").FillAsync(cliente);
        await _page.Locator("#Fecha").FillAsync(fecha.ToString("yyyy-MM-dd"));
        await _page.Locator("#Observaciones").FillAsync(observaciones);

        await GuardarFormularioAsync();
    }

    // Crear ("Crear Embarque y Configurar Checklist") y Editar ("Guardar Cambios")
    // tienen un único botón submit dentro del form.form-card.
    private async Task GuardarFormularioAsync()
    {
        await _page
            .Locator("form.form-card button[type='submit']")
            .ClickAsync();
    }

    // =====================================================
    // VERIFICACIONES EN EL LISTADO
    // =====================================================

    public async Task VerificarMensajeCreacionAsync()
    {
        await Assertions.Expect(
            _page.GetByText($"{Nombre} creada correctamente.",
                new() { Exact = true })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarEmbarqueEnListaAsync(
        string referencia,
        string cliente,
        string? pais = null)
    {
        var fila = FilaDeEmbarque(referencia);

        await Assertions.Expect(fila).ToBeVisibleAsync();
        await Assertions.Expect(fila).ToContainTextAsync(cliente);

        if (pais != null)
            await Assertions.Expect(fila).ToContainTextAsync(pais);
    }

    // Al crear el embarque el procedimiento almacenado genera los documentos del
    // checklist: la insignia de WinMovers debe mostrar "0 / N" con N mayor a cero.
    public async Task VerificarChecklistConfiguradoAsync(string referencia)
    {
        var fila = FilaDeEmbarque(referencia);

        await Assertions.Expect(fila.Locator("span.badge").First)
            .ToHaveTextAsync(new Regex(@"^\s*0 / [1-9]\d*\s*$"));
    }

    public async Task VerificarCajasYKilosEnListaAsync(
        string referencia,
        int cajas,
        decimal kilos)
    {
        var fila = FilaDeEmbarque(referencia);

        await Assertions.Expect(fila).ToBeVisibleAsync();

        // Columnas: Cliente, Referencia, Fecha, Cajas, Kilos...
        await Assertions.Expect(fila.Locator("td").Nth(3))
            .ToHaveTextAsync(cajas.ToString());

        string textoKilos = await fila.Locator("td").Nth(4).InnerTextAsync();

        AssertMonto(textoKilos, kilos, "Kilos en el listado");
    }

    // =====================================================
    // VERIFICACIONES EN LA PANTALLA DE CHECKLIST (resumen del embarque)
    // =====================================================

    public async Task VerificarResumenAsync(
        string cliente,
        string referencia,
        DateTime fecha,
        int cajas,
        decimal kilos,
        string? observaciones = null)
    {
        await Assertions.Expect(_page.Locator("h2.content-title"))
            .ToContainTextAsync($"Checklist — {cliente}");

        await Assertions.Expect(Resumen("Referencia:"))
            .ToContainTextAsync(referencia);

        await Assertions.Expect(Resumen("Fecha:"))
            .ToContainTextAsync(fecha.ToString("dd/MM/yyyy"));

        await VerificarCajasYKilosEnResumenAsync(cajas, kilos);

        if (observaciones != null)
        {
            await Assertions.Expect(_page.Locator("div.form-card").First)
                .ToContainTextAsync(observaciones);
        }
    }

    public async Task VerificarCajasYKilosEnResumenAsync(int cajas, decimal kilos)
    {
        await Assertions.Expect(Resumen("Cajas:"))
            .ToHaveTextAsync(new Regex($@"^\s*Cajas:\s*{cajas}\s*$"));

        string textoKilos = await Resumen("Kilos:").InnerTextAsync();

        AssertMonto(textoKilos, kilos, "Kilos en el resumen");
    }

    // =====================================================
    // CHECKLIST DE DOCUMENTOS
    // =====================================================

    private ILocator CasillasDocumentos =>
        _page.Locator("input[name='documentosCompletados']");

    // Marca los primeros N documentos (los del checklist de WinMovers).
    // Se hace clic en la etiqueta, que funciona aunque el check esté estilizado.
    public async Task MarcarPrimerosDocumentosAsync(int cantidad)
    {
        int total = await CasillasDocumentos.CountAsync();

        Assert.That(total, Is.GreaterThanOrEqualTo(cantidad),
            "El checklist no tiene documentos suficientes. " +
            "Revisa que Catalogo_Documentos tenga registros activos.");

        for (int i = 0; i < cantidad; i++)
        {
            var casilla = CasillasDocumentos.Nth(i);
            string? id = await casilla.GetAttributeAsync("id");

            await _page.Locator($"label[for='{id}']").ClickAsync();

            await Assertions.Expect(casilla).ToBeCheckedAsync();
        }
    }

    public async Task GuardarChecklistAsync()
    {
        await _page
            .GetByRole(AriaRole.Button, new() { Name = "Guardar Checklist" })
            .ClickAsync();
    }

    public async Task VerificarChecklistGuardadoAsync(int documentosMarcados)
    {
        await Assertions.Expect(
            _page.GetByText("Checklist actualizado correctamente.",
                new() { Exact = true })
        ).ToBeVisibleAsync();

        // La página se recarga desde la base de datos: los marcados persisten
        await Assertions.Expect(_page.Locator(".doc-status.complete"))
            .ToHaveCountAsync(documentosMarcados);

        for (int i = 0; i < documentosMarcados; i++)
            await Assertions.Expect(CasillasDocumentos.Nth(i)).ToBeCheckedAsync();
    }

    // =====================================================
    // DOCUMENTOS ADJUNTOS (PDF / imágenes)
    // =====================================================

    public async Task AdjuntarArchivoAsync(string rutaArchivo)
    {
        await _page.Locator("#inputArchivo").SetInputFilesAsync(rutaArchivo);

        await _page
            .GetByRole(AriaRole.Button, new() { Name = "Adjuntar" })
            .ClickAsync();

        // El mensaje se muestra ~1 s y luego la página se recarga
        await Assertions.Expect(_page.Locator("#msg-carga"))
            .ToContainTextAsync("Documento cargado correctamente.");
    }

    public async Task VerificarArchivoAdjuntoAsync(string nombreArchivo)
    {
        var fila = FilaDeArchivo(nombreArchivo);

        await Assertions.Expect(fila).ToBeVisibleAsync();
        await Assertions.Expect(fila).ToContainTextAsync("PDF");
    }

    public async Task EliminarArchivoAsync(string nombreArchivo)
    {
        var fila = FilaDeArchivo(nombreArchivo);

        await Assertions.Expect(fila).ToBeVisibleAsync();

        // El botón usa confirm() del navegador: hay que aceptarlo explícitamente
        _page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        await fila
            .GetByRole(AriaRole.Button, new() { Name = "Eliminar" })
            .ClickAsync();

        // La fila se quita de la tabla sin recargar la página
        await Assertions.Expect(fila).ToHaveCountAsync(0);
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    private ILocator FilaDeEmbarque(string referencia)
    {
        return _page
            .Locator("table.data-table tbody tr")
            .Filter(new LocatorFilterOptions { HasText = referencia });
    }

    private ILocator FilaDeArchivo(string nombreArchivo)
    {
        return _page
            .Locator("#tablaArchivos tr")
            .Filter(new LocatorFilterOptions { HasText = nombreArchivo });
    }

    // Bloque "Etiqueta: valor" del resumen: <div><strong>Cajas:</strong> 25</div>
    private ILocator Resumen(string etiqueta)
    {
        return _page.Locator(
            $"xpath=//div[strong[normalize-space()='{etiqueta}']]");
    }

    private static string Num(decimal valor) =>
        valor.ToString(CultureInfo.InvariantCulture);

    // El servidor da formato a los números según su cultura (1,250.50 / 1.250,50),
    // así que se comparan solo los dígitos en centavos.
    private static void AssertMonto(string textoMostrado, decimal esperado, string campo)
    {
        string esperadoCentavos = ((long)Math.Round(esperado * 100)).ToString();

        Assert.That(
            Regex.Replace(textoMostrado, @"\D", ""),
            Is.EqualTo(esperadoCentavos),
            $"{campo}: se leyó '{textoMostrado.Trim()}'.");
    }
}
