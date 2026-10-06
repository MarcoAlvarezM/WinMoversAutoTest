using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages;

public class UsuariosPage
{
    private readonly IPage _page;

    public UsuariosPage(IPage page)
    {
        _page = page;
    }

    public async Task IrAUsuariosAsync()
    {
        await _page.GotoAsync("/Usuario");
    }

    public async Task CambiarRolDeUsuarioAsync(string correo)
    {
        var filaUsuario = _page
            .Locator("tr")
            .Filter(new LocatorFilterOptions
            {
                HasText = correo
            });

        await filaUsuario
            .GetByRole(AriaRole.Link, new()
            {
                Name = "Cambiar rol"
            })
            .ClickAsync();
    }

    public async Task VerificarRolUsuarioAsync(
        string correo,
        string nombreRol)
    {
        var filaUsuario = _page
            .Locator("tr")
            .Filter(new LocatorFilterOptions
            {
                HasText = correo
            });

        await Assertions.Expect(
            filaUsuario.GetByText(nombreRol)
        ).ToBeVisibleAsync();
    }
}