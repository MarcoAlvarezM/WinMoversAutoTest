using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;

namespace WinMovers.Tests.Tests;

public class ClientesTests : TestBase
{
    [Test]
    [Category("Clientes")]
    public async Task TC_CLI_001_CrearCliente()
    {
        var loginPage = new LoginPage(Page);
        var clientesPage = new ClientesPage(Page);

        // =====================================================
        // Datos de prueba
        // =====================================================

        string nombreCliente =
            $"Cliente QA {DateTime.Now:yyyyMMddHHmmss}";

        const string telefonoCelular = "88887777";
        const string telefonoResidencia = "22223333";
        const string telefonoEmpresa = "24445555";
        const string empresa = "QA Automation";
        const string contacto = "Contacto QA";
        const string direccion = "San José, Costa Rica";
        const string correoElectronico = "qa@winmovers.test";
        const string observaciones =
            "Cliente creado mediante prueba E2E.";

        // =====================================================
        // 1. Iniciar sesión
        // =====================================================

        await loginPage.NavigateAsync();

        await loginPage.LoginAsync(
            "admin@winmovers.com",
            "Admin123!");

        await Page.WaitForURLAsync(
            "**https://localhost:7058/");

        // =====================================================
        // 2. Ir a Clientes
        // =====================================================

        await clientesPage.IrAClientesAsync();

        await Page.WaitForURLAsync(
            "**/Cliente");

        await clientesPage.VerificarPaginaClientesAsync();

        // =====================================================
        // 3. Abrir Nuevo Cliente
        // =====================================================

        await clientesPage.IrACrearClienteAsync();

        await Page.WaitForURLAsync(
            "**/Cliente/Create");

        await clientesPage.VerificarPaginaCrearClienteAsync();

        // =====================================================
        // 4. Crear cliente
        // =====================================================

        await clientesPage.CrearClienteAsync(
            nombreCliente,
            telefonoCelular,
            telefonoResidencia,
            telefonoEmpresa,
            empresa,
            contacto,
            direccion,
            correoElectronico,
            observaciones);

        // =====================================================
        // 5. Verificar redirección
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Cliente");

        // =====================================================
        // 6. Verificar mensaje de éxito
        // =====================================================

        await clientesPage.VerificarMensajeExitoAsync();

        // =====================================================
        // 7. Verificar cliente creado
        // =====================================================

        await clientesPage.VerificarClienteAsync(
            nombreCliente);
    }

    [Test]
    [Category("Clientes")]
    public async Task TC_CLI_002_EditarCliente()
    {
        var loginPage = new LoginPage(Page);
        var clientesPage = new ClientesPage(Page);

        // =====================================================
        // Datos originales
        // =====================================================

        string nombreOriginal =
            $"Cliente QA EDIT {DateTime.Now:yyyyMMddHHmmss}";

        // =====================================================
        // Datos modificados
        // =====================================================

        string nombreEditado =
            $"{nombreOriginal} MODIFICADO";

        const string telefonoCelularOriginal = "88887777";
        const string telefonoCelularEditado = "89998888";

        const string telefonoResidenciaOriginal = "22223333";
        const string telefonoResidenciaEditado = "23334444";

        const string telefonoEmpresaOriginal = "24445555";
        const string telefonoEmpresaEditado = "25556666";

        const string empresaOriginal = "QA Automation";
        const string empresaEditada = "QA Automation Editada";

        const string contactoOriginal = "Contacto QA";
        const string contactoEditado = "Contacto QA Modificado";

        const string direccionOriginal = "San José, Costa Rica";
        const string direccionEditada = "Heredia, Costa Rica";

        const string correoOriginal = "qa.edit.test@winmovers.test";
        const string correoEditado = "qa.editado@winmovers.test";

        const string observacionesOriginal =
            "Cliente creado para prueba de edición.";

        const string observacionesEditadas =
            "Cliente actualizado mediante prueba E2E.";

        // =====================================================
        // 1. Iniciar sesión
        // =====================================================

        await loginPage.NavigateAsync();

        await loginPage.LoginAsync(
            "admin@winmovers.com",
            "Admin123!");

        await Page.WaitForURLAsync(
            "**https://localhost:7058/");

        // =====================================================
        // 2. Ir a Clientes
        // =====================================================

        await clientesPage.IrAClientesAsync();

        await Page.WaitForURLAsync(
            "**/Cliente");

        await clientesPage.VerificarPaginaClientesAsync();

        // =====================================================
        // 3. Crear cliente que será utilizado para editar
        // =====================================================

        await clientesPage.IrACrearClienteAsync();

        await Page.WaitForURLAsync(
            "**/Cliente/Create");

        await clientesPage.VerificarPaginaCrearClienteAsync();

        await clientesPage.CrearClienteAsync(
            nombreOriginal,
            telefonoCelularOriginal,
            telefonoResidenciaOriginal,
            telefonoEmpresaOriginal,
            empresaOriginal,
            contactoOriginal,
            direccionOriginal,
            correoOriginal,
            observacionesOriginal);

        // =====================================================
        // 4. Verificar cliente creado
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Cliente");

        await clientesPage.VerificarClienteAsync(
            nombreOriginal);

        // =====================================================
        // 5. Abrir edición del cliente
        // =====================================================

        await clientesPage.IrAEditarClienteAsync(
            nombreOriginal);

        await Page.WaitForURLAsync(
            "**/Cliente/Edit/*");

        // =====================================================
        // 6. Verificar página de edición
        // =====================================================

        await clientesPage.VerificarPaginaEditarClienteAsync(
            nombreOriginal);

        // =====================================================
        // 7. Modificar cliente
        // =====================================================

        await clientesPage.EditarClienteAsync(
            nombreEditado,
            telefonoCelularEditado,
            telefonoResidenciaEditado,
            telefonoEmpresaEditado,
            empresaEditada,
            contactoEditado,
            direccionEditada,
            correoEditado,
            observacionesEditadas);

        // =====================================================
        // 8. Verificar redirección
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Cliente");

        // =====================================================
        // 9. Verificar mensaje de actualización
        // =====================================================

        await clientesPage.VerificarMensajeActualizacionAsync();

        // =====================================================
        // 10. Verificar datos modificados
        // =====================================================

        await clientesPage.VerificarClienteAsync(
            nombreEditado);

        // =====================================================
        // 11. Verificar que el nombre anterior ya no existe
        // =====================================================

        await clientesPage.VerificarClienteNoExisteAsync(
            nombreOriginal);
    }

    [Test]
    [Category("Clientes")]
    public async Task TC_CLI_003_BuscarClientePorNombre()
    {
        var loginPage = new LoginPage(Page);
        var clientesPage = new ClientesPage(Page);

        // =====================================================
        // Datos de prueba
        // =====================================================

        string nombreCliente =
            $"Cliente QA BUSQUEDA {DateTime.Now:yyyyMMddHHmmss}";

        const string telefonoCelular = "87776666";
        const string telefonoResidencia = "21112222";
        const string telefonoEmpresa = "23334444";
        const string empresa = "QA Automation";
        const string contacto = "Contacto Busqueda QA";
        const string direccion = "San José, Costa Rica";
        const string correoElectronico =
            "qa.busqueda@winmovers.test";

        const string observaciones =
            "Cliente creado para prueba de búsqueda.";

        // =====================================================
        // 1. Iniciar sesión
        // =====================================================

        await loginPage.NavigateAsync();

        await loginPage.LoginAsync(
            "admin@winmovers.com",
            "Admin123!");

        await Page.WaitForURLAsync(
            "**https://localhost:7058/");

        // =====================================================
        // 2. Ir a Clientes
        // =====================================================

        await clientesPage.IrAClientesAsync();

        await Page.WaitForURLAsync(
            "**/Cliente");

        // =====================================================
        // 3. Crear cliente de prueba
        // =====================================================

        await clientesPage.IrACrearClienteAsync();

        await Page.WaitForURLAsync(
            "**/Cliente/Create");

        await clientesPage.CrearClienteAsync(
            nombreCliente,
            telefonoCelular,
            telefonoResidencia,
            telefonoEmpresa,
            empresa,
            contacto,
            direccion,
            correoElectronico,
            observaciones);

        // =====================================================
        // 4. Verificar que regresó al listado
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Cliente");

        await clientesPage.VerificarClienteAsync(
            nombreCliente);

        // =====================================================
        // 5. Buscar cliente exclusivamente por nombre
        // =====================================================

        await clientesPage.BuscarClientePorNombreAsync(
            nombreCliente);

        // =====================================================
        // 6. Verificar resultado de búsqueda
        // =====================================================

        await clientesPage.VerificarClienteEncontradoAsync(
            nombreCliente);
    }

    [Test]
    [Category("Clientes")]
    public async Task TC_CLI_004_VisualizarHistorialDeMudanzas()
    {
        var loginPage = new LoginPage(Page);
        var clientesPage = new ClientesPage(Page);

        // =====================================================
        // 1. Iniciar sesión
        // =====================================================

        await loginPage.NavigateAsync();

        await loginPage.LoginAsync(
            "admin@winmovers.com",
            "Admin123!");

        await Page.WaitForURLAsync(
            "**https://localhost:7058/");

        // =====================================================
        // 2. Ir al módulo de Clientes
        // =====================================================

        await clientesPage.IrAClientesAsync();

        await Assertions.Expect(
            Page.Locator("h1.page-title")
        ).ToContainTextAsync("Clientes");



        // =====================================================
        // 3. Seleccionar un cliente existente
        // =====================================================

        var primeraFilaCliente = Page
            .Locator("table.data-table tbody tr")
            .First;

        await Assertions.Expect(
            primeraFilaCliente
        ).ToBeVisibleAsync();

        var nombreCliente = await primeraFilaCliente
            .Locator("td")
            .First
            .InnerTextAsync();

        nombreCliente = nombreCliente.Trim();

        // =====================================================
        // 4. Acceder al historial de mudanzas
        // =====================================================

        await clientesPage.IrAHistorialMudanzasAsync(
            nombreCliente);

        // =====================================================
        // 5. Verificar navegación
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Cliente/HistorialMudanzas/*");

        // =====================================================
        // 6. Verificar estructura del historial
        // =====================================================

        await clientesPage.VerificarHistorialMudanzasAsync();

        // =====================================================
        // 7. Verificar que la tabla contiene información
        //    o el mensaje correspondiente cuando no existen
        //    mudanzas
        // =====================================================

        var filas = Page.Locator(
            "table.data-table tbody tr");

        int cantidadFilas = await filas.CountAsync();

        if (cantidadFilas > 0)
        {
            var primeraFila = filas.First;

            await Assertions.Expect(
                primeraFila.GetByRole(
                    AriaRole.Link,
                    new()
                    {
                        Name = "Ver detalle"
                    })
            ).ToBeVisibleAsync();
        }
        else
        {
            await Assertions.Expect(
                Page.GetByText(
                    "Este cliente aún no posee mudanzas registradas."
                )
            ).ToBeVisibleAsync();
        }
    }

}
