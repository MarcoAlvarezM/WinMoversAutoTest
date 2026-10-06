using Microsoft.Playwright;
using NUnit.Framework;
using WinMovers.Tests.Infrastructure;
using WinMovers.Tests.Pages;

namespace WinMovers.Tests.Tests;

public class AuthenticationTests : TestBase
{
    [Test]
    [Category("Authentication")]
    public async Task TC_AUT_001_InicioDeSesionDeUsuario()
    {
        var loginPage = new LoginPage(Page);

        await loginPage.NavigateAsync();

        await Page.GetByLabel("Correo")
            .WaitForAsync();

        await loginPage.LoginAsync(
            "admin@winmovers.com",
            "Admin123!");

        await Page.WaitForURLAsync(
            "**https://localhost:7058/");

        Assert.That(
            Page.Url,
            Does.Contain("https://localhost:7058/"));
    }
}