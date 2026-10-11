using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;

namespace WinMoversAutoTest.Tests;

public class ImportacionesExportacionesTests : TestBase
{
    private const string NombrePdf = "Documento_Prueba_QA.pdf";

    // =====================================================
    // TC-IMP-001: Registrar una operación de importación
    // =====================================================

    [Test]
    [Category("ImportacionesExportaciones")]
    public async Task TC_IMP_001_RegistrarOperacionDeImportacion()
    {
        var ordenesPage = new OrdenesPage(Page);
        var importaciones = EmbarquesPage.Importaciones(Page);

        string cliente = $"QA Importacion {DateTime.Now:HHmmssfff}";
        const string pais = "Panamá"; // el campo solo admite letras

        // 1. Iniciar sesión
        await IniciarSesionAsync();

        // Precondición: la referencia debe ser una O.T. existente
        string numeroOT = await ordenesPage.CrearOrdenRapidaAsync();

        // 2. Ingresar al módulo y abrir el formulario de nuevo embarque
        await importaciones.IrAListadoAsync();
        await importaciones.IrANuevoEmbarqueAsync();
        await Page.WaitForURLAsync("**/Importacion/Create");

        // 3. Ingresar la información requerida y guardar
        await importaciones.CrearEmbarqueAsync(
            cliente,
            numeroOT,
            cajas: 25,
            kilos: 480.50m,
            fecha: DateTime.Today,
            observaciones: "Importación creada mediante prueba E2E.",
            pais: pais);

        // 4. Redirige al listado con mensaje de éxito
        await Page.WaitForURLAsync("**/Importacion");
        await importaciones.VerificarMensajeCreacionAsync();

        // 5. La operación aparece en el listado, con su checklist configurado
        await importaciones.VerificarEmbarqueEnListaAsync(numeroOT, cliente, pais);
        await importaciones.VerificarChecklistConfiguradoAsync(numeroOT);
    }

    // =====================================================
    // TC-IMP-002: Registrar una operación de exportación
    // =====================================================

    [Test]
    [Category("ImportacionesExportaciones")]
    public async Task TC_IMP_002_RegistrarOperacionDeExportacion()
    {
        var ordenesPage = new OrdenesPage(Page);
        var exportaciones = EmbarquesPage.Exportaciones(Page);

        string cliente = $"QA Exportacion {DateTime.Now:HHmmssfff}";

        await IniciarSesionAsync();

        // Precondición: una orden válida para asociar
        string numeroOT = await ordenesPage.CrearOrdenRapidaAsync();

        await exportaciones.IrAListadoAsync();
        await exportaciones.IrANuevoEmbarqueAsync();
        await Page.WaitForURLAsync("**/Exportacion/Create");

        await exportaciones.CrearEmbarqueAsync(
            cliente,
            numeroOT,
            cajas: 18,
            kilos: 320m,
            fecha: DateTime.Today,
            observaciones: "Exportación creada mediante prueba E2E.");

        await Page.WaitForURLAsync("**/Exportacion");
        await exportaciones.VerificarMensajeCreacionAsync();

        // La operación queda asociada a la orden por su referencia
        await exportaciones.VerificarEmbarqueEnListaAsync(numeroOT, cliente);
        await exportaciones.VerificarChecklistConfiguradoAsync(numeroOT);
    }

    // =====================================================
    // TC-IMP-003: Digitalizar una orden de trabajo internacional
    // =====================================================

    [Test]
    [Category("ImportacionesExportaciones")]
    public async Task TC_IMP_003_DigitalizarOrdenDeTrabajoInternacional()
    {
        var ordenesPage = new OrdenesPage(Page);
        var exportaciones = EmbarquesPage.Exportaciones(Page);

        string cliente = $"QA Digital {DateTime.Now:HHmmssfff}";
        DateTime fecha = DateTime.Today;
        const int cajas = 30;
        const decimal kilos = 845.25m;
        string observaciones =
            $"Orden digitalizada por prueba E2E {DateTime.Now:yyyyMMddHHmmss}.";

        await IniciarSesionAsync();

        string numeroOT = await ordenesPage.CrearOrdenRapidaAsync();

        // 1. Registrar la operación con toda la información de la orden
        await exportaciones.IrAListadoAsync();
        await exportaciones.IrANuevoEmbarqueAsync();

        await exportaciones.CrearEmbarqueAsync(
            cliente, numeroOT, cajas, kilos, fecha, observaciones);

        await Page.WaitForURLAsync("**/Exportacion");
        await exportaciones.VerificarMensajeCreacionAsync();

        // 2. La información queda almacenada en formato digital (ficha del embarque)
        await exportaciones.IrAChecklistAsync(numeroOT);
        await Page.WaitForURLAsync("**/Exportacion/Checklist/*");

        await exportaciones.VerificarResumenAsync(
            cliente, numeroOT, fecha, cajas, kilos, observaciones);

        // 3. El checklist digital de documentos se puede completar y se conserva
        await exportaciones.MarcarPrimerosDocumentosAsync(2);
        await exportaciones.GuardarChecklistAsync();

        await Page.WaitForURLAsync("**/Exportacion/Checklist/*");
        await exportaciones.VerificarChecklistGuardadoAsync(2);
    }

    // =====================================================
    // TC-IMP-004: Registrar datos operativos de bultos y kilos
    // =====================================================

    [Test]
    [Category("ImportacionesExportaciones")]
    public async Task TC_IMP_004_RegistrarBultosYKilos()
    {
        var ordenesPage = new OrdenesPage(Page);
        var exportaciones = EmbarquesPage.Exportaciones(Page);

        const int nuevasCajas = 40;
        const decimal nuevosKilos = 650.75m;

        await IniciarSesionAsync();

        // Precondición: una operación internacional registrada (10 cajas, 100 kilos)
        var (numeroOT, _) = await CrearEmbarquePreviaAsync(
            ordenesPage, exportaciones, cajas: 10, kilos: 100m);

        // 1. Seleccionar la operación
        await exportaciones.IrAEditarAsync(numeroOT);
        await Page.WaitForURLAsync("**/Exportacion/Edit/*");
        await exportaciones.VerificarPaginaEditarAsync();

        // 2. Ingresar bultos y kilos, y guardar
        await exportaciones.EditarCajasYKilosAsync(nuevasCajas, nuevosKilos);

        // 3. Redirige a la ficha del embarque con los datos nuevos
        await Page.WaitForURLAsync("**/Exportacion/Checklist/*");
        await exportaciones.VerificarCajasYKilosEnResumenAsync(
            nuevasCajas, nuevosKilos);

        // 4. Y el listado los refleja
        await exportaciones.IrAListadoAsync();
        await exportaciones.VerificarCajasYKilosEnListaAsync(
            numeroOT, nuevasCajas, nuevosKilos);
    }

    // =====================================================
    // TC-IMP-005: Adjuntar documentación del servicio internacional
    // (se ejecuta una vez por módulo)
    // =====================================================

    [TestCase("Importacion")]
    [TestCase("Exportacion")]
    [Category("ImportacionesExportaciones")]
    public async Task TC_IMP_005_AdjuntarDocumentacion(string modulo)
    {
        var ordenesPage = new OrdenesPage(Page);
        var embarques = new EmbarquesPage(Page, modulo);

        string rutaPdf = Path.Combine(
            TestContext.CurrentContext.TestDirectory, "TestData", NombrePdf);

        Assert.That(File.Exists(rutaPdf), Is.True,
            $"No se encontró el PDF de prueba en '{rutaPdf}'. " +
            "Copia el archivo a TestData y márcalo como 'Copiar si es posterior'.");

        await IniciarSesionAsync();

        // Precondición: una operación internacional registrada
        var (numeroOT, _) = await CrearEmbarquePreviaAsync(
            ordenesPage, embarques, cajas: 12, kilos: 150m);

        // 1. Seleccionar la operación y abrir su checklist
        await embarques.IrAChecklistAsync(numeroOT);
        await Page.WaitForURLAsync($"**/{modulo}/Checklist/*");

        // 2. Seleccionar el PDF y confirmar la carga
        await embarques.AdjuntarArchivoAsync(rutaPdf);

        // 3. Tras recargar, el documento aparece asociado al servicio
        await embarques.VerificarArchivoAdjuntoAsync(NombrePdf);

        // 4. Limpieza: eliminar el documento (también prueba el escenario de borrado)
        await embarques.EliminarArchivoAsync(NombrePdf);
    }

    // =====================================================
    // TC-IMP-006: Gestionar información general del servicio internacional
    // (requiere un usuario administrador)
    // =====================================================

    [Test]
    [Category("ImportacionesExportaciones")]
    public async Task TC_IMP_006_GestionarInformacionGeneral()
    {
        var ordenesPage = new OrdenesPage(Page);
        var exportaciones = EmbarquesPage.Exportaciones(Page);

        string nuevoCliente = $"QA Cliente Editado {DateTime.Now:HHmmssfff}";
        DateTime nuevaFecha = DateTime.Today.AddDays(7);
        string nuevasObservaciones =
            $"Información actualizada por prueba E2E {DateTime.Now:yyyyMMddHHmmss}.";

        await IniciarSesionAsync();

        var (numeroOT, _) = await CrearEmbarquePreviaAsync(
            ordenesPage, exportaciones, cajas: 10, kilos: 100m);

        // 1. Seleccionar una operación existente
        await exportaciones.IrAEditarAsync(numeroOT);
        await Page.WaitForURLAsync("**/Exportacion/Edit/*");
        await exportaciones.VerificarPaginaEditarAsync();

        // 2. Modificar la información general y guardar
        await exportaciones.EditarInformacionGeneralAsync(
            nuevoCliente, nuevaFecha, nuevasObservaciones);

        // 3. La ficha del embarque muestra la información actualizada
        await Page.WaitForURLAsync("**/Exportacion/Checklist/*");
        await exportaciones.VerificarResumenAsync(
            nuevoCliente, numeroOT, nuevaFecha, 10, 100m, nuevasObservaciones);
    }

    // =====================================================
    // HALLAZGO: en Importación el botón "Editar" no abre el formulario
    // El controlador solo tiene Edit por POST (no hay GET), así que se espera
    // que este test FALLE mientras el defecto exista. Si falla, es un hallazgo
    // para reportar, no un error del test.
    // =====================================================

    [Test]
    [Category("Hallazgo")]
    public async Task HALLAZGO_Importacion_EditarEmbarqueAbreFormulario()
    {
        var ordenesPage = new OrdenesPage(Page);
        var importaciones = EmbarquesPage.Importaciones(Page);

        await IniciarSesionAsync();

        var (numeroOT, _) = await CrearEmbarquePreviaAsync(
            ordenesPage, importaciones, cajas: 10, kilos: 100m);

        await importaciones.IrAEditarAsync(numeroOT);
        await Page.WaitForURLAsync("**/Importacion/Edit/*");

        await importaciones.VerificarPaginaEditarAsync();
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    // Crea una O.T. y un embarque asociado (precondición de varios casos).
    // Termina en el listado del módulo.
    private async Task<(string NumeroOT, string Cliente)> CrearEmbarquePreviaAsync(
        OrdenesPage ordenesPage,
        EmbarquesPage embarques,
        int cajas,
        decimal kilos)
    {
        string cliente = $"QA Embarque {DateTime.Now:HHmmssfff}";

        string numeroOT = await ordenesPage.CrearOrdenRapidaAsync();

        await embarques.IrAListadoAsync();
        await embarques.IrANuevoEmbarqueAsync();

        await embarques.CrearEmbarqueAsync(
            cliente,
            numeroOT,
            cajas,
            kilos,
            DateTime.Today,
            "Embarque de precondición creado por prueba E2E.",
            pais: embarques.EsImportacion ? "Panamá" : null);

        await embarques.VerificarMensajeCreacionAsync();

        return (numeroOT, cliente);
    }
}
