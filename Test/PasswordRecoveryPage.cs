using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages;

public class PasswordRecoveryPage
{
    private readonly IPage _page;

    public PasswordRecoveryPage(IPage page)
    {
        _page = page;
    }

    public async Task IrARecuperarContrasenaAsync()
    {
        await _page.GotoAsync("/Account/ForgotPassword");
    }

    public async Task IngresarCorreoAsync(string correo)
    {
        await _page
            .GetByLabel("Correo")
            .FillAsync(correo);
    }

    public async Task EnviarInstruccionesAsync()
    {
        await _page
            .GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Enviar instrucciones"
                })
            .ClickAsync();
    }

    public async Task VerificarConfirmacionAsync()
    {
        await Assertions.Expect(
            _page.GetByRole(
                AriaRole.Heading,
                new()
                {
                    Name = "Recuperar contraseña"
                })
        ).ToBeVisibleAsync();

        await Assertions.Expect(
            _page.GetByText(
                "Si el correo ingresado está registrado, te enviamos instrucciones para restablecer tu contraseña."
            )
        ).ToBeVisibleAsync();
    }
}