using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages;

public class ClientesPage
{
    private readonly IPage _page;

    public ClientesPage(IPage page)
    {
        _page = page;
    }

    // =====================================================
    // NAVEGACIÓN
    // =====================================================

    public async Task IrAClientesAsync()
    {
        await _page.GotoAsync("/Cliente");
    }

    public async Task IrACrearClienteAsync()
    {
        await _page
            .GetByRole(
                AriaRole.Link,
                new()
                {
                    Name = "Nuevo Cliente"
                })
            .ClickAsync();
    }

    // =====================================================
    // VERIFICACIONES DE PÁGINA
    // =====================================================

    public async Task VerificarPaginaClientesAsync()
    {
        await Assertions.Expect(
            _page.Locator("h1.page-title")
        ).ToContainTextAsync("Clientes");
    }

    public async Task VerificarPaginaCrearClienteAsync()
    {
        await Assertions.Expect(
            _page.Locator("h1.page-title")
        ).ToContainTextAsync("Nuevo Cliente");
    }

    // =====================================================
    // CREAR CLIENTE
    // =====================================================

    public async Task CrearClienteAsync(
        string nombre,
        string telefonoCelular,
        string telefonoResidencia,
        string telefonoEmpresa,
        string empresa,
        string contacto,
        string direccion,
        string correoElectronico,
        string observaciones)
    {
        await _page
            .Locator("#NombreCliente")
            .FillAsync(nombre);

        await _page
            .Locator("#TelefonoCelular")
            .FillAsync(telefonoCelular);

        await _page
            .Locator("#TelefonoResidencia")
            .FillAsync(telefonoResidencia);

        await _page
            .Locator("#TelefonoEmpresa")
            .FillAsync(telefonoEmpresa);

        await _page
            .Locator("#Empresa")
            .FillAsync(empresa);

        await _page
            .Locator("#Contacto")
            .FillAsync(contacto);

        await _page
            .Locator("#Direccion")
            .FillAsync(direccion);

        await _page
            .Locator("#CorreoElectronico")
            .FillAsync(correoElectronico);

        await _page
            .Locator("#Observaciones")
            .FillAsync(observaciones);

        await _page
            .GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Guardar Cliente"
                })
            .ClickAsync();
    }

    // =====================================================
    // EDITAR CLIENTE
    // =====================================================

    public async Task IrAEditarClienteAsync(string nombreCliente)
    {
        var filaCliente = _page
            .Locator("tr")
            .Filter(new LocatorFilterOptions
            {
                HasText = nombreCliente
            });

        await Assertions.Expect(
            filaCliente
        ).ToBeVisibleAsync();

        var enlaceEditar = filaCliente
            .Locator("a[href*='/Cliente/Edit/']");

        await Assertions.Expect(
            enlaceEditar
        ).ToBeVisibleAsync();

        await enlaceEditar.ClickAsync();
    }



    public async Task VerificarPaginaEditarClienteAsync(
    string nombreCliente)
    {
        await Assertions.Expect(
            _page.Locator("h1.page-title")
        ).ToContainTextAsync("Editar Cliente");

        await Assertions.Expect(
            _page.Locator("h2.content-title")
        ).ToContainTextAsync("Editar Cliente");

        await Assertions.Expect(
            _page.Locator("body")
        ).ToContainTextAsync(nombreCliente);
    }


    public async Task EditarClienteAsync(
        string nombre,
        string telefonoCelular,
        string telefonoResidencia,
        string telefonoEmpresa,
        string empresa,
        string contacto,
        string direccion,
        string correoElectronico,
        string observaciones)
    {
        await _page
            .Locator("#NombreCliente")
            .FillAsync(nombre);

        await _page
            .Locator("#TelefonoCelular")
            .FillAsync(telefonoCelular);

        await _page
            .Locator("#TelefonoResidencia")
            .FillAsync(telefonoResidencia);

        await _page
            .Locator("#TelefonoEmpresa")
            .FillAsync(telefonoEmpresa);

        await _page
            .Locator("#Empresa")
            .FillAsync(empresa);

        await _page
            .Locator("#Contacto")
            .FillAsync(contacto);

        await _page
            .Locator("#Direccion")
            .FillAsync(direccion);

        await _page
            .Locator("#CorreoElectronico")
            .FillAsync(correoElectronico);

        await _page
            .Locator("#Observaciones")
            .FillAsync(observaciones);

        await _page
            .GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Guardar Cambios"
                })
            .ClickAsync();
    }

    // =====================================================
    // VERIFICACIONES
    // =====================================================

    public async Task VerificarClienteAsync(string nombre)
    {
        await Assertions.Expect(
            _page.GetByText(nombre, new()
            {
                Exact = true
            })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarMensajeExitoAsync()
    {
        await Assertions.Expect(
            _page.GetByText(
                "Cliente creado correctamente.",
                new()
                {
                    Exact = true
                })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarMensajeActualizacionAsync()
    {
        await Assertions.Expect(
            _page.GetByText(
                "Cliente actualizado correctamente.",
                new()
                {
                    Exact = true
                })
        ).ToBeVisibleAsync();
    }

    public async Task VerificarClienteNoExisteAsync(
        string nombre)
    {
        await Assertions.Expect(
            _page.GetByText(
                nombre,
                new()
                {
                    Exact = true
                })
        ).Not.ToBeVisibleAsync();
    }

    public async Task BuscarClientePorNombreAsync(string nombreCliente)
    {
        await _page
            .Locator("input[name='nombreCliente']")
            .FillAsync(nombreCliente);

        await _page
            .GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Buscar"
                })
            .ClickAsync();
    }

    public async Task VerificarClienteEncontradoAsync(
        string nombreCliente)
    {
        var filaCliente = _page
            .Locator("tbody tr")
            .Filter(new LocatorFilterOptions
            {
                HasText = nombreCliente
            });

        await Assertions.Expect(
            filaCliente
        ).ToBeVisibleAsync();
    }

    public async Task IrAHistorialMudanzasAsync(string nombreCliente)
    {
        var filaCliente = _page
            .Locator("table.data-table tbody tr")
            .Filter(new LocatorFilterOptions
            {
                HasText = nombreCliente
            });

        await Assertions.Expect(filaCliente)
            .ToBeVisibleAsync();

        var enlaceHistorial = filaCliente.Locator(
            "a[href*='/Cliente/HistorialMudanzas/']"
        );

        await Assertions.Expect(enlaceHistorial)
            .ToBeVisibleAsync();

        await enlaceHistorial.ClickAsync();
    }


    public async Task VerificarHistorialMudanzasAsync()
    {
        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Columnheader,
                new()
                {
                    Name = "O.T."
                })
        ).ToBeVisibleAsync();

        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Columnheader,
                new()
                {
                    Name = "Fecha Servicio"
                })
        ).ToBeVisibleAsync();

        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Columnheader,
                new()
                {
                    Name = "Estado"
                })
        ).ToBeVisibleAsync();

        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Columnheader,
                new()
                {
                    Name = "Monto"
                })
        ).ToBeVisibleAsync();

        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Columnheader,
                new()
                {
                    Name = "Acción"
                })
        ).ToBeVisibleAsync();
    }


}
