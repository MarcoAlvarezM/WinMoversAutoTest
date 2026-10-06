using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages;

public class RolesPage
{
    private readonly IPage _page;

    public RolesPage(IPage page)
    {
        _page = page;
    }

    public async Task IrACrearRolAsync()
    {
        await _page.GotoAsync("/Rol/Create");
    }

    public async Task CrearRolAsync(string nombre, string descripcion)
    {
        await _page.GetByLabel("Nombre").FillAsync(nombre);
        await _page.GetByLabel("Descripción").FillAsync(descripcion);

        await _page
            .GetByRole(AriaRole.Button, new()
            {
                Name = "Crear rol"
            })
            .ClickAsync();
    }

    public async Task AsignarRolAsync(string nombreRol)
    {
        // El formulario real usa:
        // <select asp-for="NombreRol">
        var selectorRol = _page.Locator("select[name='NombreRol']");

        await selectorRol.SelectOptionAsync(
            new SelectOptionValue
            {
                Label = nombreRol
            });

        await _page
            .GetByRole(AriaRole.Button, new()
            {
                Name = "Asignar rol"
            })
            .ClickAsync();
    }
}