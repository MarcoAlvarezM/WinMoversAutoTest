using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages;

public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("http://3.15.121.133/");
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