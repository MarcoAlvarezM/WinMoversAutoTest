using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Infrastructure;
using WinMoversAutoTest.Pages;

namespace WinMoversAutoTest;

public class AuthenticationTests : TestBase
{
    [Test]
    [Category("Authentication")]
    public async Task TC_AUT_003_AdministracionDeRolesDeUsuarios()
    {
        var loginPage = new LoginPage(Page);
        var rolesPage = new RolesPage(Page);
        var usuariosPage = new UsuariosPage(Page);

        const string correoUsuario = "marcoalvmejia@gmail.com";
        const string nombreUsuario = "Marco Álvarez Mejía";

        string nombreRol =
            $"QA_Automated_Role_{DateTime.Now:yyyyMMddHHmmss}";

        const string descripcionRol =
            "Rol creado automáticamente mediante prueba E2E.";

        // =====================================================
        // 1. Iniciar sesión como administrador
        // =====================================================

        await loginPage.NavigateAsync();

        await loginPage.LoginAsync(
            "admin@winmovers.com",
            "Admin123!");

        await Page.WaitForURLAsync("**https://localhost:7058/");

        // =====================================================
        // 2. Entrar al módulo de roles y crear nuevo rol
        // =====================================================

        await rolesPage.IrACrearRolAsync();

        await Assertions.Expect(
            Page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Crear nuevo rol" })
        ).ToBeVisibleAsync();

        await rolesPage.CrearRolAsync(
            nombreRol,
            descripcionRol);

        // La creación redirige a /Rol
        await Page.WaitForURLAsync("**/Rol");

        // Verificamos que el rol realmente apareció
        await Assertions.Expect(
            Page.Locator("span.badge.badge-info")
                .Filter(new LocatorFilterOptions
                {
                    HasText = nombreRol
                })
        ).ToBeVisibleAsync();


        // =====================================================
        // 3. Ir a usuarios
        // =====================================================

        await usuariosPage.IrAUsuariosAsync();

        await Assertions.Expect(
            Page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Usuarios del sistema" })
        ).ToBeVisibleAsync();

        // =====================================================
        // 4. Buscar usuario y cambiar rol
        // =====================================================

        await usuariosPage.CambiarRolDeUsuarioAsync(
            correoUsuario);

        // Debe llevarnos a:
        // /Rol/AsignarRol/{id}

        await Page.WaitForURLAsync("**/Rol/AsignarRol/*");

        // Verificar que estamos en la página de asignación de roles
        await Assertions.Expect(
            Page.Locator("h2.content-title")
        ).ToContainTextAsync("Asignar rol");

        // Verificar que estamos modificando el usuario correcto
        await Assertions.Expect(
            Page.GetByText(nombreUsuario, new()
            {
                Exact = true
            })
        ).ToBeVisibleAsync();

        // Verificar que estamos modificando el usuario correcto.
        // La página de asignación muestra el nombre, no el correo.
        await Assertions.Expect(
            Page.GetByText(nombreUsuario, new()
            {
                Exact = true
            })
        ).ToBeVisibleAsync();


        // =====================================================
        // 5. Seleccionar el nuevo rol y guardar
        // =====================================================

        await rolesPage.AsignarRolAsync(nombreRol);

        // El controlador redirige a /Usuario
        await Page.WaitForURLAsync("**/Usuario");

        // =====================================================
        // 6. Verificar que el usuario tiene el nuevo rol
        // =====================================================

        await usuariosPage.VerificarRolUsuarioAsync(
            correoUsuario,
            nombreRol);

        // =====================================================
        // 7. Verificar mensaje de éxito
        // =====================================================

        await Assertions.Expect(
            Page.GetByText(
                $"Rol asignado correctamente a"
            )
        ).ToBeVisibleAsync();
    }

    [Test]
    [Category("Authentication")]
    public async Task TC_AUT_002_RecuperacionDeContrasena()
    {
        var loginPage = new LoginPage(Page);
        var passwordRecoveryPage = new PasswordRecoveryPage(Page);

        const string correo =
            "marcoalvmejia@gmail.com";

        // =====================================================
        // 1. Ir a la pantalla de inicio de sesión
        // =====================================================

        await loginPage.NavigateAsync();

        await Assertions.Expect(
            Page.GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Iniciar sesión"
                })
        ).ToBeVisibleAsync();

        // =====================================================
        // 2. Seleccionar "¿Olvidaste tu contraseña?"
        // =====================================================

        await Page.GetByRole(
            AriaRole.Link,
            new()
            {
                Name = "¿Olvidaste tu contraseña?"
            })
            .ClickAsync();

        // =====================================================
        // 3. Verificar pantalla de recuperación
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Account/ForgotPassword");

        await Assertions.Expect(
            Page.GetByRole(
                AriaRole.Heading,
                new()
                {
                    Name = "Recuperar contraseña"
                })
        ).ToBeVisibleAsync();

        await Assertions.Expect(
            Page.GetByText(
                "Ingresa tu correo y te enviaremos instrucciones para restablecerla"
            )
        ).ToBeVisibleAsync();

        // =====================================================
        // 4. Ingresar correo
        // =====================================================

        await passwordRecoveryPage.IngresarCorreoAsync(
            correo);

        // =====================================================
        // 5. Enviar instrucciones
        // =====================================================

        await passwordRecoveryPage.EnviarInstruccionesAsync();

        // =====================================================
        // 6. Verificar confirmación
        // =====================================================

        await Page.WaitForURLAsync(
            "**/Account/ForgotPasswordConfirmation");

        await passwordRecoveryPage.VerificarConfirmacionAsync();
    }

}