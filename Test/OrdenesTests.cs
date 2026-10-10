using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;

namespace WinMoversAutoTest.Tests;

public class OrdenesTests : TestBase
{

    // =====================================================
    // TC-ORD-001: Crear una orden de mudanza
    // =====================================================

    [Test]
    [Category("Ordenes")]
    public async Task TC_ORD_001_CrearOrdenDeMudanza()
    {
        var ordenesPage = new OrdenesPage(Page);

        string numeroOT = $"QA-{DateTime.Now:yyyyMMddHHmmss}";
        DateTime fechaServicio = FechaServicioLibre();

        // 1. Iniciar sesión
        await IniciarSesionAsync();

        // 2. Ir al módulo de órdenes
        await ordenesPage.IrAOrdenesAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo");

        // 3. Seleccionar Nueva orden
        await ordenesPage.IrANuevaOrdenAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo/Create");

        // 4. Completar los datos y guardar
        await ordenesPage.CrearOrdenAsync(
            numeroOT,
            fechaServicio,
            "09:30",
            $"Cliente QA {numeroOT}",
            "88887777",
            "San José, Costa Rica",
            "Heredia, Costa Rica",
            "Orden creada mediante prueba E2E.",
            "Cliente QA",
            "QA Automation");

        // 5. Verificar redirección y mensaje de éxito
        await Page.WaitForURLAsync("**/OrdenTrabajo");
        await ordenesPage.VerificarMensajeCreacionAsync();

        // 6. La orden aparece en la lista con estado inicial "Pendiente"
        await ordenesPage.VerificarOrdenEnListaAsync(
            numeroOT,
            "Pendiente",
            fechaServicio);
    }

    // =====================================================
    // TC-ORD-002: Asignar fecha de servicio a una orden
    // =====================================================

    [Test]
    [Category("Ordenes")]
    public async Task TC_ORD_002_AsignarFechaDeServicio()
    {
        var ordenesPage = new OrdenesPage(Page);

        await IniciarSesionAsync();

        // Precondición: una orden creada (la crea el propio test para no
        // depender de los datos que haya en la base de datos).
        string numeroOT = await CrearOrdenPreviaAsync(ordenesPage);

        // Nueva fecha de servicio, distinta a la original
        DateTime nuevaFecha = FechaServicioLibre();

        // 1. Seleccionar la orden
        await ordenesPage.IrAEditarOrdenAsync(numeroOT);
        await ordenesPage.VerificarPaginaEditarOrdenAsync(numeroOT);

        // 2. Asignar la fecha y guardar
        await ordenesPage.AsignarFechaServicioAsync(nuevaFecha);
        await ordenesPage.GuardarFormularioAsync();

        // 3. Verificar redirección, mensaje y fecha actualizada
        await Page.WaitForURLAsync("**/OrdenTrabajo");
        await ordenesPage.VerificarMensajeActualizacionAsync();

        await ordenesPage.VerificarOrdenEnListaAsync(
            numeroOT,
            "Pendiente",
            nuevaFecha);

        // 4. Auditoría: el cambio queda en el historial (campo "Fecha de Servicio")
        await ordenesPage.IrAEditarOrdenAsync(numeroOT);
        await ordenesPage.IrAHistorialDesdeEdicionAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo/Historial/*");

        await ordenesPage.VerificarHistorialContieneAsync(
            "Fecha de Servicio",
            nuevaFecha.ToString("yyyy-MM-dd"));
    }

    // =====================================================
    // TC-ORD-003: Actualizar el estado de una orden
    // =====================================================

    [Test]
    [Category("Ordenes")]
    public async Task TC_ORD_003_ActualizarEstadoDeOrden()
    {
        var ordenesPage = new OrdenesPage(Page);

        await IniciarSesionAsync();

        string numeroOT = await CrearOrdenPreviaAsync(ordenesPage);

        // 1. Seleccionar la orden
        await ordenesPage.IrAEditarOrdenAsync(numeroOT);
        await ordenesPage.VerificarPaginaEditarOrdenAsync(numeroOT);

        // 2. Cambiar el estado (el sistema solo maneja Pendiente / Completado)
        await ordenesPage.CambiarEstadoAsync("Completado");
        await ordenesPage.GuardarFormularioAsync();

        // 3. Verificar redirección, mensaje y estado en la lista
        await Page.WaitForURLAsync("**/OrdenTrabajo");
        await ordenesPage.VerificarMensajeActualizacionAsync();

        await ordenesPage.VerificarOrdenEnListaAsync(numeroOT, "Completado");

        // 4. Auditoría: estado anterior y nuevo en el historial
        await ordenesPage.IrAEditarOrdenAsync(numeroOT);
        await ordenesPage.IrAHistorialDesdeEdicionAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo/Historial/*");

        await ordenesPage.VerificarHistorialContieneAsync(
            "Estado",
            "Pendiente",
            "Completado");
    }

    // =====================================================
    // TC-ORD-004: Agregar notas y observaciones a una orden
    // =====================================================

    [Test]
    [Category("Ordenes")]
    public async Task TC_ORD_004_AgregarNotasYObservaciones()
    {
        var ordenesPage = new OrdenesPage(Page);

        string textoNota =
            $"Nota QA {DateTime.Now:yyyyMMddHHmmss}: cliente pide empacar la vajilla aparte.";

        await IniciarSesionAsync();

        string numeroOT = await CrearOrdenPreviaAsync(ordenesPage);

        // 1. Seleccionar la orden y abrir sus notas
        await ordenesPage.IrAEditarOrdenAsync(numeroOT);
        await ordenesPage.IrANotasDesdeEdicionAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo/Notas/*");

        // 2. Ingresar la nota y guardar
        await ordenesPage.AgregarNotaAsync(textoNota);

        // 3. Verificar redirección y mensaje
        await Page.WaitForURLAsync("**/OrdenTrabajo/Notas/*");
        await ordenesPage.VerificarMensajeNotaAgregadaAsync();

        // 4. La nota aparece en el historial con usuario y fecha
        await ordenesPage.VerificarNotaEnHistorialAsync(textoNota);
    }

    // =====================================================
    // AUXILIARES
    // =====================================================

    // El sistema no deja crear dos órdenes con la misma fecha de servicio,
    // así que se usa una fecha lejana y aleatoria para no chocar con datos existentes.
    private static DateTime FechaServicioLibre()
    {
        return DateTime.Today.AddDays(400 + Random.Shared.Next(0, 3000));
    }

    private async Task<string> CrearOrdenPreviaAsync(OrdenesPage ordenesPage)
    {
        string numeroOT = $"QA-{DateTime.Now:yyyyMMddHHmmssfff}";

        await ordenesPage.IrAOrdenesAsync();
        await ordenesPage.IrANuevaOrdenAsync();
        await Page.WaitForURLAsync("**/OrdenTrabajo/Create");

        await ordenesPage.CrearOrdenAsync(
            numeroOT,
            FechaServicioLibre(),
            "08:00",
            $"Cliente QA {numeroOT}",
            "88887777",
            "San José, Costa Rica",
            "Cartago, Costa Rica",
            "Orden de precondición creada por prueba E2E.",
            "Cliente QA",
            "QA Automation");

        await Page.WaitForURLAsync("**/OrdenTrabajo");
        await ordenesPage.VerificarMensajeCreacionAsync();

        return numeroOT;
    }
}
