using Microsoft.Playwright;

namespace WinMovers.Tests.Pages;

public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/");
    }

    public async Task LoginAsync(string email, string password)
    {
        await _page.GetByLabel("Correo").FillAsync(email);
        await _page.GetByLabel("Contraseña").FillAsync(password);

        await _page
            .GetByRole(AriaRole.Button, new()
            {
                Name = "Iniciar sesión"
            })
            .ClickAsync();
    }
}